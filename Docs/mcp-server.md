# ManiaAPI.NET MCP server design

Status: proposal. Target: .NET 11, MCP spec revision **2026-07-28** (stateless core), MCP C# SDK **2.x**.

## Goals

1. **The supported surface is invokable.** Every public operation of the libraries listed below can be called through MCP, about 270 distinct library operations before game-specific catalog descriptors.
2. **The tool list stays small.** A client that connects pays a few thousand tokens for tool definitions, not the ~68k tokens that 270 separate tools would cost.
3. **Stateless.** Every request stands on its own, as the 2026-07-28 revision requires. Any instance behind a load balancer can serve any request, with no sticky sessions.
4. **One codebase, two ways to run it:**
   - locally over stdio, launched by an MCP client
   - on a server over Streamable HTTP
5. **Credentials stay on the server.** Logins, passwords, and OAuth client secrets come from configuration. They never appear in tool definitions, arguments, or results, so the model never sees them.

### Non-goals (for v1)

- **Acting as an end user** with their own Ubisoft or Trackmania login, for example `/user/...` in the Trackmania API or `/me` in ManiaPlanet. See [User-delegated access](#user-delegated-access-later).

### Excluded integrations

- **GBXRemote integration.** Its persistent dedicated-server connections are outside this stateless MCP design.
- **TrackmaniaIO integration.** It is excluded from the MCP catalog, respecting the site maintainer's preference against AI/MCP use.

## The size of the surface

The counts come from the libraries' client interfaces. Overloads are versions of one method with different parameter types. Each operation exposes one canonical overload, chosen as described in [Overloads](#overloads).

| Toolset | Library and client | Operations | Overloads |
|---|---|---:|---:|
| `trackmania2020` | NadeoAPI `NadeoServices` (core) | 30 | 42 |
| | NadeoAPI `NadeoLiveServices` | 83 | 94 |
| | NadeoAPI `NadeoMeetServices` | 30 | 30 |
| | TrackmaniaAPI (public OAuth API) | 9 | 13 |
| `exchange` | TMX: TMUF, TMNF, Nations, Sunrise, Original | 13 | 17 |
| | MX: Trackmania, ManiaPlanet, Shootmania | 22 | 28 |
| | TMX/MX Gbx extensions | 12 | 24 |
| `tmuf` | MasterServerTMUF (United) | — | — |
| | TrackmaniaWS, UnitedLadder | 2 | 2 |
| `tmnf` | MasterServerTMUF (Nations) | shared with `tmuf` | shared with `tmuf` |
| `maniaplanet` | MasterServerMP3, MasterServerMP4 | — | — |
| | ManiaPlanet web API and in-game API | 14 | 17 |
| `trackmaniaturbo` | MasterServerTMT, AggregatedMasterServerTMT | — | — |

The MasterServer rows together cover roughly 55 distinct operations and 110 overloads, before exposing the shared TMUF/TMNF operations separately for each game. Per-game counts will come from the generated catalog.

At roughly 250 tokens per tool definition (name, description, input schema), exposing every supported library operation as its own tool would cost about **68k tokens** before the conversation starts, with more for game-specific descriptors. Most clients slow down or degrade well before that point.

## Approach: a small fixed tool set over an operation catalog

The server has three layers:

```
┌───────────────────────────────────────────────────────────────┐
│  MCP surface (what tools/list returns)                          │
│   • 3 gateway tools:  maniaapi_search · maniaapi_describe ·     │
│                       maniaapi_invoke                           │
│   • ~10 featured tools per toolset (configurable)               │
├───────────────────────────────────────────────────────────────┤
│  Operation engine                                               │
│   argument binding · account selection · write gate ·           │
│   result shaping · error mapping · rate limiting                │
├───────────────────────────────────────────────────────────────┤
│  Operation catalog (generated at build time)                    │
│   one descriptor per operation: id, toolset, summary, input     │
│   schema, annotations, auth needs, typed invoker delegate       │
├───────────────────────────────────────────────────────────────┤
│  ManiaAPI.NET clients (registered through the Hosting packages)  │
└───────────────────────────────────────────────────────────────┘
```

### Gateway tools

These three tools are always listed, whatever the configuration. Together they cost about 1.5k tokens.

#### `maniaapi_search`

Finds operations by keywords, so the model doesn't need the whole catalog.

```jsonc
// input
{ "query": "map leaderboard top", "game": "trackmania2020", "limit": 8 }
```

The result is one compact line per operation. Each line has the operation's ID, signature, summary, and flags:

```
nadeo.live.top_leaderboard(mapUid: string, groupUid?: string, zoneId?: guid, length?: int = 10, offset?: int = 0, onlyWorld?: bool = true) — Top records of a map, worldwide or in one zone. [read]
nadeo.live.surrounding_records(groupUid: string, mapUid: string, lower: int, upper: int, score?: int, onlyWorld?: bool, zoneId?: string) — Records around a score. [read]
nadeo.live.club_top_leaderboard(groupUid: string, mapUid: string, clubId: int, length?: int = 5, offset?: int = 0) — Top records among a club's members. [read]
```

Simple parameter types are written inline, so most calls can go straight from `search` to `invoke` without `describe`. Results rank by BM25 over the operation ID, summary, parameter names, and tags. The index is built at startup from the catalog and has no outside dependency. Filters:

- `toolset`
- `game`: accepts canonical game names and [game aliases](#game-aliases-and-title-context), and includes matching game operations and relevant exchange operations available in the current scope
- `readOnly`
- `requires`: `none`, `nadeo`, or `oauth`

`toolset` filters by the operation's owning toolset; `game` searches across toolsets using the [game-to-exchange mapping](#game-aware-exchange-discovery). For example, searching for community maps with `game: "trackmania2020"` can return MX operations for the Trackmania site. Exchange signatures include the allowed sites and game names, so the model can choose the right source without guessing.

For example, `{ "query": "community maps", "game": "TM2", "limit": 8 }` resolves to the `maniaplanet` toolset with a Trackmania 2 family hint. It can discover ManiaPlanet operations and MX `Maniaplanet` operations; the exchange site choices shown for that search exclude `Trackmania` and `Shootmania`.

The `game` filter narrows the site choices shown in that search result. It doesn't change the scope of later calls or create session state; invocations still supply `site` unless the endpoint itself has a single-site default.

#### `maniaapi_describe`

Returns the full details of up to 10 operations:

- the JSON Schema of the input
- an outline of the result's fields, two levels deep
- which accounts the operation needs, and which are known to be denied (taken from the live tests)
- for OAuth operations, the upstream provider, required grant type, and scopes
- pagination parameters
- supported games, allowed exchange sites, when to use the operation, and related operations available in the current scope
- one example call

Results are worth caching on the client side, because the catalog can't change while a deployment runs. Cache by endpoint and operation ID, since allowed sites and schemas differ between endpoints.

#### `maniaapi_invoke`

Runs one operation.

```jsonc
{
  "operation": "nadeo.live.top_leaderboard",
  "arguments": { "mapUid": "UFOjPIpxep2VpcWBntgTtmVgfuc", "length": 5 },
  "shape": {                      // optional; see Result shaping
    "fields": ["top.top[].accountId", "top.top[].score", "top.top[].position"],
    "maxItems": 50,
    "maxChars": 20000
  },
  "account": "ServiceAccount",    // optional; NadeoAPI only, defaults per operation
  "confirm": false                // needed for writes when the client can't answer an MRTR prompt
}
```

The engine checks `arguments` against the operation's schema before calling anything, and reports errors per field. Annotations are per tool, not per call, so the `maniaapi_invoke` tool has these:

| Annotation | Value |
|---|---|
| `readOnlyHint` | `false` |
| `destructiveHint` | `true` when the deployment allows writes, otherwise `false` |
| `openWorldHint` | `true` |

### Featured tools

Common tasks get first-class tools, so the model can do them in one call and with a precise output schema. Each featured tool is a thin, hand-written wrapper over one or more catalog operations. It may combine calls: for example, `trackmania2020_player` resolves a display name to an account ID, then returns the zone, trophy summary, and club tag.

Suggested starting set, about 17 tools and 5k tokens when every toolset is enabled:

| Tool | Built on |
|---|---|
| `trackmania2020_player` | TrackmaniaAPI display names, NadeoServices zones, trophies, club tags |
| `trackmania2020_map` | NadeoServices or NadeoLiveServices map info, medal records |
| `trackmania2020_leaderboard` | NadeoLiveServices top, surrounding, and position-by-time leaderboards |
| `trackmania2020_campaign` | NadeoLiveServices seasonal, weekly, and club campaigns |
| `trackmania2020_track_of_the_day` | NadeoLiveServices track of the day, plus NadeoMeetServices cup of the day |
| `trackmania2020_club` | NadeoLiveServices club, members, activities |
| `trackmania2020_matchmaking` | NadeoMeetServices summary, rankings, player ranks |
| `exchange_search` | TMX tracks, MX maps; use for community map discovery, with sites restricted by the endpoint's game scope |
| `exchange_item` | TMX track, MX map or mappack details, author information, and file links; use to enrich a known map with exchange data |
| `maniaplanet_title` | ManiaPlanet web API titles, in-game API title download info |
| `tmuf_player`, `tmnf_player` | MasterServerTMUF player infos and login check, bound to the respective game's server |
| `tmuf_leaderboard`, `tmnf_leaderboard` | MasterServerTMUF campaign and map leaderboard summaries, bound to the respective game's server |
| `maniaplanet_player` | MasterServerMP4 player infos and login check |
| `maniaplanet_leaderboard` | ManiaPlanet MasterServer campaign and map leaderboard summaries |
| `trackmaniaturbo_leaderboard` | MasterServerTMT campaign and map leaderboard summaries, for one platform or aggregated across platforms |

Featured tools are configured with `Featured: [...]` per toolset. Featured tools of disabled toolsets aren't listed. Game endpoints also list the configured exchange featured tools when at least one mapped site is available; their descriptions and site schemas use that endpoint's scope.

### Why this approach

- **Splitting into several MCP servers isn't enough on its own.** The `trackmania2020` toolset alone has 152 operations, so a per-library server is still far too big.
- **Client-side tool search** (where a client loads definitions only when the model asks) works with some clients but not all. The server should work well everywhere, so the gateway is the default. For capable clients, `ExposeOperationsAsTools: true` lists every operation of the enabled toolsets as its own tool (named after the operation ID, with `.` replaced by `_`), and the client's tool search keeps the context small.
- **Search, describe, invoke** puts the catalog behind three cheap calls, and the compact signatures usually remove the `describe` step.

## Toolsets and endpoints

Game toolsets are `trackmania2020`, `tmuf` (Trackmania United Forever), `tmnf` (Trackmania Nations Forever), `maniaplanet`, and `trackmaniaturbo`. ManiaPlanet includes Trackmania 2 and ShootMania through its MP3 and MP4 clients and shared web APIs. The `exchange` toolset covers map and replay services used across games. A deployment chooses which toolsets are enabled. In a stateless server, the toolset can't depend on session state, so the choice comes from the endpoint path (HTTP) or a command-line option (stdio):

| Endpoint | Toolsets | Purpose |
|---|---|---|
| `/mcp` | every enabled toolset | general use |
| `/mcp/trackmania2020` | `trackmania2020` + scoped `exchange` | Trackmania (2020), plus MX Trackmania |
| `/mcp/exchange` | `exchange` | all configured exchange sites |
| `/mcp/tmuf` | `tmuf` + scoped `exchange` | Trackmania United Forever, plus TMX United Forever |
| `/mcp/tmnf` | `tmnf` + scoped `exchange` | Trackmania Nations Forever, plus TMX Nations Forever |
| `/mcp/maniaplanet` | `maniaplanet` + scoped `exchange` | Trackmania 2 and ShootMania through ManiaPlanet APIs, plus their MX sites |
| `/mcp/trackmaniaturbo` | `trackmaniaturbo` | Trackmania Turbo, including aggregated platform results |

- **stdio:** the same selection comes from the `ManiaApiMcp:Toolsets` setting, or `--toolsets trackmania2020,exchange` on the command line.
- **Scoping:** gateway tools only search, describe, and invoke operations of the endpoint's effective toolsets. A game endpoint adds the relevant subset of `exchange` only when that toolset is enabled and a mapped site is configured. Site restrictions apply to featured tools, per-operation tools, invocations, and resource reads as well as discovery.
- **Shared clients:** TMUF and TMNF use the same library interface, but each has its own catalog descriptors and featured tools bound to the United or Nations server. The `tmuf` toolset also includes TrackmaniaWS and UnitedLadder. A game-specific endpoint cannot select the other game's server through an argument.
- **List caching:** `tools/list` returns a fixed result per endpoint, so it sets the cacheable-list fields from the 2026-07-28 revision: a long `ttlMs`, and a `cacheScope` that lets every user of the endpoint share the cached list.
- **Gateway routing:** the `Mcp-Method` and `Mcp-Name` headers let a gateway route or rate-limit by tool without reading the body.

### Game aliases and title context

The MCP catalog has an explicit alias registry used by the `game` filter and discovery metadata. Resolve names case-insensitively, normalize Unicode compatibility characters (including `²` to `2`), and ignore spacing and punctuation when matching registered aliases:

| Accepted game names | Canonical toolset | Family hint | Exchange site |
|---|---|---|---|
| `trackmania2`, `Trackmania 2`, `TrackMania²`, `TM2` | `maniaplanet` | Trackmania 2 | MX `Maniaplanet` |
| `shootmania`, `ShootMania`, `SM` | `maniaplanet` | ShootMania | MX `Shootmania` |
| `maniaplanet` | `maniaplanet` | unspecified; use title or game context | MX `Maniaplanet` or `Shootmania` |

Alias resolution preserves the family hint alongside the canonical toolset. A TM2 request therefore remains a Trackmania 2 request when selecting exchange operations, even though it uses the shared ManiaPlanet backend. Toolset names, endpoint paths, and operation IDs stay canonical; `toolset` filters and configuration use `maniaplanet`.

Preserve an explicitly supplied title ID or environment in the search query and carry it into subsequent calls using the parameters each operation supports. Resolving a game alias must not discard title context or invent a title ID. An environment name such as `Stadium` alone is ambiguous: use explicit game or title context when available, otherwise return the ambiguity and guide the model to ask which game the user means.

Gateway and ManiaPlanet featured-tool descriptions include the TM2 and ShootMania aliases and explain that Trackmania 2 uses ManiaPlanet APIs and MX `Maniaplanet`, and ShootMania uses ManiaPlanet APIs and MX `Shootmania`. Search tags include the same aliases. This makes natural-language queries discoverable even when the caller doesn't supply a `game` filter.

Resolution stays within the endpoint's effective scope and never enables a disabled toolset or changes endpoints automatically. A TM2020-only endpoint receiving `game: "TM2"` reports that the request requires `maniaplanet` and directs the client to `/mcp/maniaplanet` or `/mcp` with ManiaPlanet enabled. Unknown game names return a validation error with supported choices. No family or title context is stored between calls.

### Game-aware exchange discovery

The catalog records which exchange sites belong to each game. Site values use the existing library enums:

| Game toolset | Exchange client and site | Selection guidance |
|---|---|---|
| `trackmania2020` | MX `Trackmania` | community maps for Trackmania (2020) |
| `tmuf` | TMX `TMUF` | tracks for Trackmania United Forever |
| `tmnf` | TMX `TMNF` | tracks for Trackmania Nations Forever |
| `maniaplanet` | MX `Maniaplanet`, MX `Shootmania` | choose by the resolved family hint or title: Trackmania 2 uses `Maniaplanet`, ShootMania uses `Shootmania` |
| `trackmaniaturbo` | none in the supported clients | no exchange operations added |

For game endpoints, the allowed sites are the intersection of this mapping and `Exchange:Sites` / `Exchange:MxSites`. Disabled or unmapped sites aren't advertised or callable there. `/mcp` and `/mcp/exchange`, and stdio with `exchange` selected, expose all configured exchange sites, including TMX `Nations`, `Sunrise`, and `Original`.

The gateway tool descriptions returned by `tools/list` state the current game scope, available exchange sites, and when exchange lookups help: community map searches, author information, replays, or downloads where supported. Search tags include game names and exchange synonyms. `maniaapi_describe` gives the site choices and related operations, filtered to those actually available. Descriptions guide the model toward an exchange lookup when useful; game calls don't automatically fetch exchange data.

On a game endpoint with exactly one allowed site for an exchange operation, `site` can be omitted and is bound to that site. When more than one site is allowed, the caller must select one. For ManiaPlanet, use the resolved family hint or title information to choose the site; if the game is still ambiguous, ask the user. A prior `game: "TM2"` search doesn't change invocation defaults: pass `site: "Maniaplanet"` when the endpoint allows both sites. Invocations reject sites outside the endpoint's allowed set, even if the caller supplies one explicitly.

## Operation catalog

### Generation

A Roslyn incremental source generator, `ManiaAPI.MCP.Generators`, reads an explicit allowlist of supported client interfaces in referenced assemblies (`INadeoServices`, `INadeoLiveServices`, `ITMX`, `IMX`, `IMasterServerTMUF`, …) and emits:

- one `OperationDescriptor` per operation:
  - ID and toolset
  - supported games and exchange sites, plus usage guidance and related operation IDs
  - authentication requirements, including OAuth provider, grant type, and scopes where applicable
  - summary, from the XML doc's `<summary>`, with a fallback written from the method name
  - parameter list with JSON Schema, defaults, and descriptions from the XML doc's `<param>`
  - result type
  - a typed invoker, `Func<IServiceProvider, JsonElement, CancellationToken, ValueTask<object?>>`, which needs no reflection
- a `JsonSerializerContext` with `[JsonSerializable]` for every result and parameter type. The libraries' own contexts are `internal`, so the host needs its own. The property attributes (`JsonPropertyName`, `JsonConverter`) live on the model types, so they still apply.

This keeps the host trim- and Native-AOT-compatible, like the libraries (`IsAotCompatible`). Startup needs no reflection.

Hand-written partial `OperationOverrides` files hold what generation can't infer:

- better summaries
- tags and synonyms for search ("TOTD", "COTD", "PB", "WR")
- game aliases and family hints, game-to-exchange mappings, site selection guidance, and related operations for discovery
- the account requirements per NadeoAPI operation, taken from the live tests (for example, `GetPlayerClubTagsAsync` returns 401 for a dedicated server account)
- excluded operations
- which operations write

The libraries themselves stay unaware of MCP.

### Operation IDs

The format is `<library>.<client>.<snake_case_name>`, with `Get` and `Async` removed:

| Method | Operation ID |
|---|---|
| `NadeoLiveServices.GetTopLeaderboardAsync` | `nadeo.live.top_leaderboard` |
| `NadeoServices.GetPlayerZonesAsync` | `nadeo.core.player_zones` |
| `MX.SearchMapsAsync` | `mx.search_maps`, plus a `site` argument (inferred on a game endpoint with exactly one allowed site) |
| `TMX.GetReplaysAsync` | `tmx.replays`, plus a `site` argument (inferred on a game endpoint with exactly one allowed site) |
| `MasterServerTMUF.GetLadderPlayerRankingsAsync` | `masterserver.tmuf.ladder_player_rankings` |
| `MasterServerTMUF.GetLadderPlayerRankingsAsync` (Nations server) | `masterserver.tmnf.ladder_player_rankings` |

Clients that exist once per site or platform (`TMX`, `MX`, `MasterServerTMT`) get the site or platform as an ordinary argument. The engine creates or reuses the matching client. Exchange `site` schemas are narrowed to the allowed sites in the current scope; the argument is required except for the single-site game endpoint case described above.

### Overloads

Each operation exposes one canonical overload:

1. **Skip:** `CancellationToken` (the engine supplies the request's token) and `params T[]` overloads when an `IEnumerable<T>` version exists.
2. **Prefer** the overload with the most optional parameters. For example, `GetAccountRecordsByMapIdsAsync(accountId, mapIds, gamemode, offset)` over the version without `offset`.
3. **Pick per argument when two overloads differ only in identifier type**, like MasterServer's `string league` and `int leagueId`. The schema accepts `oneOf` both, and the invoker calls the overload that matches the value.
4. **Merge item overloads.** Overloads that take an item (`TrackItem`, `MapItem`) or its ID collapse into the ID form.
5. **Merge overloads with different parameters into one schema.** Every parameter that isn't in all of them becomes optional. The invoker picks the overload from the arguments it receives, and an override file states the rule:
   - `GetTopLeaderboardAsync` has three overloads: `mapUid` only, with `groupUid`, and with `groupUid` plus `zoneId`. A `zoneId` selects the zone overload, and it can't be combined with `length` or `offset`.
   
   A combination that no overload accepts is a schema error listing the valid ones.

### Excluded or adapted members

| Member | Treatment |
|---|---|
| `AuthorizeAsync`, `RefreshAsync`, `DisposeAsync` | Handled by the host; not operations |
| Obsolete members (`GetWeeklyCampaignsAsync`, `GetAccountDisplayNamesAsync`, …) | Listed with `[obsolete]` and ranked last. `GetAccountDisplayNamesAsync` is excluded, since the API no longer has its route |
| `UploadMapAsync`, `UpdateMapAsync` (NadeoAPI Gbx extension) | Available as writes, under the same rules as every other [write](#writes-and-safety). The map file is passed as a local `.Map.Gbx` path (stdio only), a URL, or a `resource` reference instead of a stream |
| `ManiaPlanetIngameAPI.AuthenticateAsync` | Excluded; online servers use it to verify players, and a separate service handles that |
| Methods returning `HttpResponseMessage`, `Stream`, or `Gbx<T>` | See [Binary results](#binary-results) |

## Result shaping

API responses can be large: a club activity list, a TMUF scores file, a full map record list. Shaping keeps them inside the context window.

1. **Serialize** with the generated `JsonSerializerContext`, in camelCase.
2. **Project** if `shape.fields` is given. It takes dotted paths with `[]` for arrays, like `top.top[].accountId`, and anything else is dropped. Featured tools have tuned default projections.
3. **Limit arrays.** Each array is cut to `shape.maxItems` items (default 50). The result notes where it was cut, for example `"…": "112 more items; pass offset=50 to continue"`, using the pagination parameters from the descriptor.
4. **Cap the size.** The whole result is cut to `shape.maxChars` (default 20k characters, with a server-wide maximum from configuration). A note says what was dropped.
5. **Return both forms.** The result goes out as `structuredContent` and as text JSON for older clients. Featured tools declare an `outputSchema`. Gateway invocations don't, because their shape depends on the operation.

### Binary results

Map and replay files, images, title packs, and TMUF scores files are never returned inline.

**`HttpResponseMessage` and `Stream`:** the result holds the status, content type, length, and the upstream URL where the library knows it. It also includes a resource link such as `maniaapi://tmx/tmnf/tracks/2233/gbx` or `maniaapi://mx/tm/maps/120839/thumbnail`. `resources/read` downloads the file again when asked, up to `MaxResourceBytes` (default 5 MB). Images come back as `blob` content, so a client with vision can show them. The URI contains everything needed to fetch the file again, so no state is kept between requests.

**`Gbx<T>` and Gbx nodes:** the engine reads the header only and returns a summary instead of the object graph:

- maps: UID, name, author, medal times, environment
- replays: map UID, player, time

The resource link is still included for the file itself.

## Errors

Library exceptions become tool results with `isError: true`, a short message, and a hint the model can act on:

| Exception | Example result |
|---|---|
| `NadeoAPIResponseException` 401 with a dedicated server account | `Denied for the DedicatedServer account. This operation needs a service account: pass "account": "ServiceAccount".` |
| `NadeoAPIResponseException` 403 (club activities) | `Only for members of club 146883.` |
| `TrackmaniaAPIResponseException` 400 | The API's message, with the 50-names-per-request limit pointed out |
| `HttpRequestException` 404 (UnitedLadder, TMWS) | `Player 'x' not found.` |
| `XmlRequestException` (MasterServer) | The master server's error code and text, such as `Error 7: The provided login is invalid.` |
| Missing configuration | `This operation needs NadeoAPI credentials, which this server doesn't have.` |
| Game resolves outside the endpoint's scope | `TM2 uses the maniaplanet toolset. Use /mcp/maniaplanet or /mcp with ManiaPlanet enabled.` |

Schema validation errors list each invalid argument. Upstream timeouts and 5xx responses are reported with `retryable: true`.

## Credentials and configuration

All credentials live in the `ManiaApiMcp` configuration section. Locally they come from user secrets or environment variables set in the MCP client's server entry. On a server they come from any `IConfiguration` provider: environment variables, key-per-file container secrets (`/run/secrets`), Azure Key Vault, AWS Secrets Manager, and so on.

```jsonc
{
  "ManiaApiMcp": {
    "Toolsets": ["trackmania2020", "exchange", "tmuf", "tmnf", "maniaplanet", "trackmaniaturbo"],
    "Http": { "ApiKeys": [] },   // populate from secrets for HTTP hosting
    "AllowWrites": false,        // local only: stdio, or HTTP bound to loopback
    "ExposeOperationsAsTools": false,
    "Shaping": { "MaxItems": 50, "MaxChars": 20000, "MaxCharsLimit": 100000, "MaxResourceBytes": 5242880 },

    "NadeoAPI": {
      "UserAgent": "my-assistant (contact: you@example.com)",
      "Accounts": {
        "ServiceAccount":  { "Login": "", "Password": "" },   // from secrets
        "DedicatedServer": { "Login": "", "Password": "" }    // optional
      },
      "DefaultAccount": "ServiceAccount"
    },
    "TrackmaniaApi": { "ClientId": "", "ClientSecret": "", "Scopes": [] },
    "ManiaPlanet":   { "ClientId": "", "ClientSecret": "", "Scopes": [] },   // optional
    "TrackmaniaWS":  { "ApiUsername": "", "ApiPassword": "" },
    "Exchange":      { "Sites": ["TMNF", "TMUF", "Nations", "Sunrise", "Original"], "MxSites": ["Trackmania", "Maniaplanet", "Shootmania"] },
    "MasterServer":  { "Games": ["TMUF", "TMNF", "MP4", "TMT"] }
  }
}
```

- **Operations without credentials are hidden.** If a credential is missing, its operations disappear from search and featured tools, and `maniaapi_invoke` reports that the server isn't configured. The other toolsets still work. ManiaPlanet credentials are optional: the application-level ManiaPlanet web API operations work without them (see [Upstream OAuth 2.0](#upstream-oauth-20)).
- **Exchange availability:** `exchange` must be enabled in `Toolsets`; `Exchange:Sites` and `Exchange:MxSites` select the available sites. Game endpoints automatically include only the mapped subset. Removing a site from configuration removes it from discovery and rejects its invocations and resource reads in every scope.
- **Clients come from the existing Hosting packages:** `AddNadeoAPI`, `AddTrackmaniaAPI`, `AddManiaPlanetAPI`, `AddTMX`, `AddMasterServer*`, `AddTrackmaniaWS`.
  - **Tokens:** NadeoAPI, Trackmania OAuth and ManiaPlanet OAuth tokens are cached for the life of the process in the singleton handlers. The handlers already authorize lazily on the first request and refresh before tokens expire. The protocol is stateless, but the process may still cache tokens.
  - **Multiple instances:** each instance authorizes on its own. That's fine for a few instances. Nadeo rate-limits authentication, so a large deployment would need a shared token cache.
- **Two NadeoAPI accounts.** Each operation's descriptor records which account it needs, as observed by the live tests. Most work with either account. Map uploads, skins, favorites, club tags and trophies need the service account. The engine picks the default, and `account` overrides it.

### Protecting the endpoint

A server with credentials behind it acts as those accounts for anyone who can reach it, so **the initial HTTP deployment uses API-key authentication**. The MCP client supplies a key in a header, checked against `ManiaApiMcp:Http:ApiKeys`. HTTP requests without a valid key are rejected, and HTTP hosting requires at least one non-empty configured key. A local stdio server needs no endpoint authentication.

This is a separate authentication layer from the game APIs:

| Connection | Authentication | Initial scope |
|---|---|---|
| MCP client → MCP HTTP server | API key | required from the first deployment |
| MCP server → TrackmaniaAPI | OAuth 2.0 client credentials | supported application-level operations, using the configured client ID, secret, and scopes |
| MCP server → ManiaPlanetAPI | none, or OAuth 2.0 client credentials when configured | application-level operations; credentials are optional |
| MCP server → player-specific TrackmaniaAPI / ManiaPlanetAPI routes | OAuth 2.0 authorization code flow with player consent | later account-linking support; these operations are excluded from v1 |

OAuth authentication for the MCP endpoint can be considered later if needed. It isn't required to use OAuth 2.0 with the upstream game APIs.

### Upstream OAuth 2.0

TrackmaniaAPI and ManiaPlanetAPI already support acquiring application tokens through the client credentials grant. The host uses those clients for operations that accept application tokens. Each catalog descriptor records its OAuth provider, grant type, and required scopes; configuration supplies the scopes needed for the enabled application-level operations.

ManiaPlanetAPI's application-level operations (titles, title scripts, maps, online servers, zones) don't require authentication. Without `ManiaPlanet` credentials, the host calls them anonymously and they stay listed. With credentials, it authorizes through the client credentials grant and sends the application token. Their descriptors record OAuth as optional, so `requires: none` search results include them.

Player-specific routes need a token associated with the player. For example, TrackmaniaAPI's user admin-club operation requires `clubs`, reading favorites requires `read_favorite`, and changing favorites requires `write_favorite`, all with a player-authorized token. Adding those scopes to a client credentials request doesn't turn the resulting application token into a player token. `AllowWrites` and write confirmation still apply separately.

Configuring a client ID and secret enables only operations supported by that grant. Player-only operations remain excluded until account linking is implemented; the host must not fall back to an application token for them.

### User-delegated access (later)

Endpoints that act as a specific player (Trackmania API `/user/...`, ManiaPlanet `/me/...`) need that player's OAuth token. In phase 4, a separate browser account-linking flow will obtain consent through the relevant provider's authorization code flow. The Hosting packages already contain the ASP.NET Core authentication handlers for both providers.

The MCP HTTP endpoint continues to authenticate callers with API keys. Linked upstream accounts and their tokens are associated with the authenticated caller's internal identity, with protected shared storage for multiple instances. Upstream access and refresh tokens stay on the server; they aren't tool arguments or substitutes for the MCP API key. This keeps upstream account linking separate from MCP endpoint authentication and doesn't require the MCP server to become its own OAuth authorization server.

## Writes and safety

- **Read-only by default.** `AllowWrites: false` (the default) hides every write operation and rejects invoking one. With `AllowWrites: true`, every write operation in the catalog is available, through `maniaapi_invoke` and per-operation tools alike. Writes include:
  - club create, edit, and delete
  - favorites and votes
  - matchmaking heartbeat and cancel
  - map upload and update
  - the Trackmania API's favorite add and remove
- **Writes are local only.** A write acts as the configured accounts, so it's allowed only when the server runs for one local user: over stdio, or over HTTP with every listening address on loopback (`localhost`, `127.0.0.1`, `[::1]`). An HTTP server reachable from other machines is always read-only. `AllowWrites: true` with a non-loopback HTTP address fails startup instead of being ignored.
- **Annotations.** Featured and per-operation tools carry `readOnlyHint`, `destructiveHint`, and `idempotentHint` from the descriptor.
- **Confirmation.** With writes allowed, a write asks for confirmation through a Multi Round-Trip Request (MRTR):
  1. The server throws `InputRequiredException` with an elicitation that describes the change, for example "Remove map X from your favorites?".
  2. The client re-sends the call with the user's answer.
  3. If the client doesn't support MRTR (`McpServer.IsMrtrSupported` is false), the call must include `"confirm": true` instead.
  
  MRTR is the 2026-07-28 replacement for server-initiated elicitation, which doesn't work in stateless mode.
- **Rate limits.**
  - **NadeoAPI:** limited to a few concurrent requests per audience.
  - **The MCP endpoint itself:** ASP.NET Core rate limiting per API key or user.

## Hosting

### Projects

The MCP host, source generator, and tests live in this repository alongside the ManiaAPI.NET libraries. The host ships as the `ManiaAPI.MCP` NuGet .NET tool package and contains the catalog types, operation engine, result shaping, and featured tools. The project paths below are relative to the repository root:

```
MCP/
  ManiaAPI.MCP/               host + operation engine: stdio + Streamable HTTP, packed as a .NET tool (net11.0)
Generators/
  ManiaAPI.MCP.Generators/    catalog + JsonSerializerContext source generator (netstandard2.0)
Tests/
  ManiaAPI.MCP.Tests/         unit tests (fake HTTP handlers, as elsewhere) + [LiveTest] smoke tests
```

The host keeps these components in separate folders within one project. Both transports share the same engine, and the tests reference the host project directly. A separate core library can be extracted later if another application needs to embed the engine.

The host references the C# SDK packages `ModelContextProtocol` (stdio, hosting) and `ModelContextProtocol.AspNetCore` (Streamable HTTP). They target up to net10.0 and run on net11.0. HTTP transport options keep the v2 default, `Stateless = true`.

### One binary, two ways to run it

```csharp
var http = args.Contains("--http");
var builder = http ? WebApplication.CreateBuilder(args) : Host.CreateApplicationBuilder(args);

builder.Services.AddManiaApiMcp(builder.Configuration);   // clients, catalog, engine, featured tools

var mcp = builder.Services.AddMcpServer()
    .WithTools<GatewayTools>()
    .WithFeaturedTools()          // filtered by enabled toolsets and configured credentials
    .WithManiaApiResources();     // maniaapi:// resource templates

if (http) mcp.WithHttpTransport();
else
{
    // stdout carries the protocol on stdio, so every log goes to stderr.
    builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
    mcp.WithStdioServerTransport();
}
```

`tools/list` depends on the endpoint path, so the filtering happens in the SDK's list and call handlers, based on the route of the current request. Tool descriptions, site schemas, and resource handlers use the same effective game and exchange scope. stdio uses the `Toolsets` setting instead. The SDK pages I read don't show a per-request filter API, so check the SDK before building on this. If there is no such hook, the fallback is one `MapMcp` endpoint per scope, each with its own tool collection, including its mapped exchange operations.

### Configuration validation

The host checks `ManiaApiMcp` at startup and fails with a message that says what to fix, without including secret values.

- **Structural checks run locally:** toolset and site names, shaping limits within `MaxCharsLimit`, and, for HTTP hosting, at least one non-empty API key and no `AllowWrites: true` unless every listening address is loopback. The validator is registered in DI and attached with .NET 11's [`OptionsBuilder<TOptions>.Validate<TValidator>()`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.options.optionsbuilder-1.validate?view=net-11.0-pp) plus `ValidateOnStart()`. Check the overload in the installed Microsoft.Extensions.Options version before relying on it.
- **Missing credentials aren't errors.** They hide the affected operations, as described in [Credentials and configuration](#credentials-and-configuration).
- **No upstream calls at startup.** .NET 11 adds [asynchronous options validation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/libraries#async-validation-with-dataannotations) and `IAsyncStartupValidator`, but the host doesn't use them to log in. Tokens stay lazy, and an upstream outage must not fail startup or change which operations an endpoint lists. Upstream health is reported by `/readyz` only. Use asynchronous validation only for a check that genuinely needs I/O, and propagate cancellation.

### HTTP surface

- **The engine validates tool arguments.** [ASP.NET Core 11](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11) Minimal API validation, including asynchronous DataAnnotations, applies to endpoint parameters, not to arguments inside an MCP tool call. The engine's JSON Schema validation runs for both transports.
- **The SDK owns MCP framing.** Streamable HTTP and its SSE responses come from `ModelContextProtocol.AspNetCore`; the host doesn't implement its own SSE endpoint for tools.
- **OpenAPI is optional.** The health endpoints don't need an OpenAPI document. If one is added for ordinary HTTP endpoints, ASP.NET Core 11 generates OpenAPI 3.2 by default; choose an older version explicitly if downstream tooling needs it. MCP tool schemas are a separate contract and don't come from OpenAPI.

### Relation to the existing Hosting packages

The host targets net11.0 only. The `*.Extensions.Hosting` packages it uses keep targeting net8.0 through net11.0, and the samples stay on net10.0; the host doesn't upgrade them. .NET 11-only validation code stays in the host. Moving it into a Hosting package requires a `NET11_0_OR_GREATER` guard with an equivalent for older targets, and a check that the referenced package versions provide the API on every target.

### Local

The host is packed as a .NET tool package, so an MCP client can start it with `dnx`. Credentials can go in the client's server entry as environment variables:

```jsonc
{
  "mcpServers": {
    "maniaapi": {
      "command": "dnx",
      "args": ["ManiaAPI.MCP", "--yes"],
      "env": {
        "ManiaApiMcp__Toolsets__0": "trackmania2020",
        "ManiaApiMcp__Toolsets__1": "exchange",
        "ManiaApiMcp__Exchange__MxSites__0": "Trackmania",
        "ManiaApiMcp__NadeoAPI__Accounts__ServiceAccount__Login": "…",
        "ManiaApiMcp__NadeoAPI__Accounts__ServiceAccount__Password": "…"
      }
    }
  }
}
```

Alternatively, `dotnet user-secrets` with the tool's `UserSecretsId`, so the secrets stay out of the client's config file. `--http --urls http://localhost:5199` runs the HTTP transport locally.

### Server

- **Container:** a `mcr.microsoft.com/dotnet/aspnet:11.0` image, or a Native AOT build on `runtime-deps:11.0` for a small image and fast cold starts.
- **Scaling:** stateless, so any number of replicas without sticky sessions. Scale-to-zero platforms (Container Apps, Cloud Run) work too.
- **Health:** `/healthz` for liveness. `/readyz` reports the state of each configured credential from the token handlers: authorized with a valid cached token, not yet authorized, or failed on its last attempt. It doesn't log in on a probe, because probes run often on every replica and Nadeo rate-limits authentication; tokens are still acquired lazily by real calls. "Not yet authorized" counts as ready, otherwise a fresh instance would never receive the traffic that authorizes it. A failure there doesn't change which tools are listed.
- **Observability:** OpenTelemetry traces and metrics per operation (latency, upstream status, shaped vs. raw size), plus a log line per call without arguments that could hold secrets.
- **Secrets:** environment variables or mounted secret files, as above.

## Testing

- **Unit tests:** the engine, shaping, overload resolution, and schema checks against the fake HTTP handlers the library tests already use. Include game-to-site mappings, single-site defaults, explicit site selection, disabled sites, and rejection of cross-game invocations and resource reads.
- **Alias resolution tests:** all TM2 aliases resolve to `maniaplanet` with the Trackmania 2 family hint, and all ShootMania aliases with the ShootMania family hint, including case, spacing, punctuation, and Unicode variants. Preserve explicit title and environment context, report ambiguous names and unknown aliases, and respect disabled toolsets and endpoint scope.
- **Generator snapshot tests:** the catalog. They catch a new or changed library method that would silently change the operation surface.
- **Startup tests:** HTTP hosting fails to start without an API key, or with `AllowWrites: true` on a non-loopback address, and invalid toolset or site names fail with messages that contain no secret values. Missing credentials start successfully with their operations hidden. A stdio run writes only protocol messages to stdout.
- **Authentication tests:** HTTP rejects missing or invalid API keys; upstream OAuth requirements are independent of endpoint authentication. Application credentials don't expose player-only operations, and descriptors report the required provider, grant, and scopes.
- **Protocol tests:** an in-process MCP client (`McpClient` from the C# SDK) calls the gateway tools through both transports. Includes a stateless HTTP round trip and an MRTR confirmation. Check that a game endpoint advertises its available exchange sites in tool descriptions and schemas, and that game-filtered searches discover relevant exchange operations without returning unrelated sites. A `game: "TM2"` search discovers ManiaPlanet operations and only MX `Maniaplanet` site choices; it doesn't affect a later invocation's required arguments. A TM2020-only endpoint returns the ManiaPlanet scope guidance.
- **Live smoke tests:** `[LiveTest]` tests reuse the existing live settings and run one featured tool per toolset.
- **Context budget test:** fails if `tools/list` for `/mcp` grows over a set number of tokens (for example 8k), so the tool list can't creep up.

## Phases

1. **Catalog and gateway:** generator, engine, the three gateway tools, read-only `trackmania2020` and `exchange` toolsets, game-aware exchange discovery and endpoint scoping, stdio and HTTP hosting, API-key protection, and upstream TrackmaniaAPI client credentials OAuth.
2. **Featured tools and binaries:** featured tools, `maniaapi://` resources, Gbx header summaries, the `tmuf`, `tmnf`, `maniaplanet`, and `trackmaniaturbo` toolsets.
3. **Writes:** writes behind `AllowWrites`, local only, with MRTR confirmation, including map upload and update.
4. **User-delegated access:** separate upstream TrackmaniaAPI and ManiaPlanetAPI OAuth account linking for player-specific requests, while retaining API-key authentication for the MCP HTTP endpoint.

## Open questions

1. Which operations should be featured first? Usage data from the library's consumers would help.
2. Should `ExposeOperationsAsTools` exist at all, or should the gateway be the only way, so behaviour is the same for every client?
