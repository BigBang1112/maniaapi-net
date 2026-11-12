# ManiaAPI.MCP

A stateless .NET 11 MCP server for ManiaAPI.NET, implementing all four phases of [the design](../../Docs/mcp-server.md). The catalog contains 367 operations, including game-specific bindings and adapted Gbx operations. GBXRemote and TrackmaniaIO remain excluded.

The default tool list contains three gateways and up to 17 featured tools. The complete configured list, including output schemas, stays within the tested 8k-token estimate.

- `maniaapi_search` finds operations by keywords, game, toolset, credentials, and read/write status.
- `maniaapi_describe` returns schemas, overloads, result outlines, authentication grants/scopes, examples, and pagination.
- `maniaapi_invoke` validates arguments, selects accounts and overloads, enforces write confirmation, and shapes results.

The featured tools cover Trackmania 2020 players, maps, leaderboards, campaigns, Track of the Day, clubs, and matchmaking; exchange search and item details; ManiaPlanet titles and players; and legacy game leaderboards. Tools with several views accept a `mode` argument shown in their input schema. Advanced exchange filters remain available through the catalog gateway.

## Run

Use the .NET 11 SDK from the repository root:

```powershell
dotnet run --project Src/ManiaAPI.MCP -- --toolsets exchange
```

Stdio is the default transport. Stdout contains protocol messages; logs go to stderr. The repository's workspace-local SDK, when present, can be used as `artifacts/dotnet11/dotnet.exe` instead of `dotnet`.

To run Streamable HTTP, configure at least one `ManiaApiMcp:Http:ApiKeys` secret, then run:

```powershell
dotnet run --project Src/ManiaAPI.MCP -- --http --urls http://localhost:5199
```

Clients send the key in `X-Api-Key`. `/mcp` exposes every enabled toolset. The game endpoints are `/mcp/trackmania2020`, `/mcp/tmuf`, `/mcp/tmnf`, `/mcp/maniaplanet`, and `/mcp/trackmaniaturbo`; `/mcp/exchange` exposes all configured exchange sites. Disabled toolsets have no mapped endpoint.

Game endpoints include their mapped exchange sites when exchange is enabled. TMUF and TMNF bind to different master servers. ManiaPlanet includes MP3, MP4, Trackmania 2, and ShootMania. Turbo supports individual platforms and aggregated results, discovering its master servers lazily.

`/healthz` is anonymous liveness. `/readyz` requires the API key and reads cached credential status without logging in. Fresh or expired credentials can authorize lazily on a real call. HTTP requests are limited to 120 per minute per API key; upstream calls are limited by NadeoAPI audience or other toolset.

## Configuration

The host loads normal .NET configuration, optional user secrets with ID `ManiaAPI.MCP`, optional key-per-file secrets from `/run/secrets`, environment variables, and command-line settings. `--toolsets` replaces the configured array. Keep credentials in secrets rather than committed JSON.

```json
{
  "ManiaApiMcp": {
    "Toolsets": ["trackmania2020", "exchange", "tmuf", "tmnf", "maniaplanet", "trackmaniaturbo"],
    "AllowWrites": false,
    "ExposeOperationsAsTools": false,
    "Http": { "ApiKeys": [] },
    "Shaping": {
      "MaxItems": 50,
      "MaxChars": 20000,
      "MaxCharsLimit": 100000,
      "MaxResourceBytes": 5242880
    },
    "NadeoAPI": {
      "UserAgent": "my-assistant (contact: you@example.com)",
      "DefaultAccount": "ServiceAccount",
      "Accounts": {}
    },
    "TrackmaniaApi": { "Scopes": [] },
    "ManiaPlanet": { "Scopes": [] },
    "TrackmaniaWS": {},
    "Exchange": {
      "Sites": ["TMUF", "TMNF", "Nations", "Sunrise", "Original"],
      "MxSites": ["Trackmania", "Maniaplanet", "Shootmania"]
    },
    "MasterServer": { "Games": ["TMUF", "TMNF", "MP3", "MP4", "TMT"] },
    "Featured": {
      "exchange": { "Tools": ["exchange_search", "exchange_item"] }
    },
    "AccountLinking": { "Enabled": false }
  }
}
```

NadeoAPI accounts use `NadeoAPI:Accounts:ServiceAccount:Login` and `Password`, with an optional `DedicatedServer` account. `TrackmaniaApi` and `ManiaPlanet` accept `ClientId` and `ClientSecret`. `TrackmaniaWS` accepts `ApiUsername` and `ApiPassword`. An environment variable such as `ManiaApiMcp__NadeoAPI__Accounts__ServiceAccount__Login` maps to the corresponding configuration key.

Missing credentials hide only the affected operations. ManiaPlanet application-level reads remain available anonymously; configured application credentials are optional for those reads. Player routes always require linked player tokens. Supplying player scopes with client credentials does not enable those routes.

Omitting a `Featured` entry enables its available defaults. Set `"Tools": []` to hide a toolset's featured tools. `ExposeOperationsAsTools` additionally exposes individual catalog operations with dots replaced by underscores. All entry points enforce the same game, site, credential, and write restrictions.

## Discovery and results

```json
{"query":"community maps","game":"TM2","limit":8}
```

`TM2`, `Trackmania 2`, and `TrackMania²` resolve to ManiaPlanet with the Trackmania 2 family hint and MX `Maniaplanet`. `ShootMania`/`SM` use MX `Shootmania`. Title/environment text remains in the query. Ambiguous environment names require explicit game or title context.

Search filters do not change later calls. General and exchange endpoints require an explicit exchange site. A game endpoint with one available site can infer it. Cross-game or disabled sites are rejected during discovery, invocation, and resource reads.

Example invocation:

```json
{
  "operation": "mx.search_maps",
  "arguments": {
    "site": "Trackmania",
    "parameters": { "name": "Stadium", "count": 5 }
  },
  "shape": {
    "fields": ["results[].mapId", "results[].name"],
    "maxItems": 5
  }
}
```

Results contain `data` and `truncation`, with identical structured and text JSON. Projection and array limits run before the character cap. Oversized results return guidance to narrow the request; JSON is never sliced inside a value. Upstream errors are sanitized and temporary failures carry `retryable: true`.

## Binary resources and Gbx

Binary operations return content metadata, upstream URLs where available, and a `maniaapi://` resource link. Resource URIs encode the operation and its arguments, so a different instance can fetch them again without session state. Resource reads enforce the current endpoint scope and `MaxResourceBytes`; images and other files return blob contents rather than inline tool-result data.

TMX/MX Gbx operations return header summaries, including map UID/name/author, medals and environment, or replay map/player/time. Full and header/node variants share this MCP adaptation instead of exposing the Gbx object graph. TM Forever scores files are downloadable resources.

## Writes

`AllowWrites` defaults to false. Enabling it exposes the write catalog, including NadeoAPI club/member/activity changes, favorites, votes, matchmaking, and map upload/update. HTTP hosting fails startup unless every actual listening address is loopback; stdio permits local writes. API-key authentication remains mandatory for HTTP, including loopback.

For clients supporting MRTR, a write prompts for approval and retries with a protected confirmation bound to the operation, arguments, account, endpoint scope, caller, and tool. Confirmation expires after five minutes. `confirm: true` is the fallback for clients without MRTR and does not bypass an MRTR prompt.

Map uploads and updates accept exactly one source:

```json
{
  "operation": "nadeo.core.upload_map",
  "arguments": { "source": { "path": "C:/Maps/Example.Map.Gbx" } },
  "confirm": true
}
```

Local paths are stdio-only. `source.url` accepts an HTTP(S) URL without embedded credentials; `source.resource` accepts a returned resource URI. Downloads and local files are bounded by `MaxResourceBytes`. Updates optionally accept `mapId`; otherwise the library resolves the map from the Gbx UID. Uploads require a NadeoAPI service account. Password-returning library operations stay excluded.

## Link player accounts

Account linking uses HTTP and is separate from MCP endpoint authentication. Enable it with provider credentials plus persistent storage:

```json
{
  "ManiaApiMcp": {
    "AccountLinking": {
      "Enabled": true,
      "PublicOrigin": "https://maniaapi.example.com",
      "StorePath": "/var/lib/maniaapi/accounts",
      "DataProtectionKeyPath": "/var/lib/maniaapi/keys",
      "TrackmaniaScopes": ["clubs", "read_favorite", "write_favorite"],
      "ManiaPlanetScopes": ["basic", "dedicated", "maps", "titles"]
    }
  }
}
```

`PublicOrigin` must use HTTPS, with HTTP allowed for loopback development. Register the matching provider callback URLs `/signin-trackmania` and `/signin-maniaplanet`. A shared deployment uses shared account storage and the same protected ASP.NET data-protection key ring; restrict directory access to the server identities.

1. Send `POST /account-links/trackmania` or `POST /account-links/maniaplanet` with the caller's `X-Api-Key`.
2. Open the returned `url` in a browser within five minutes. The existing hosting package performs OAuth authorization-code consent with state/correlation protection and PKCE.
3. Return to the MCP client. Player operations use the linked account for that API key. `GET /account-links/status` reports linked providers; `DELETE /account-links/{provider}` removes a link. Both require the API key.

The API key's hash identifies its caller, so reordering configured keys does not change ownership. Callers sharing a key share that identity. Tokens are encrypted with a caller/provider-specific protection purpose and never appear in tool arguments or results. Refreshes use a shared file lock to serialize rotating refresh tokens across instances. Player clients cannot fall back to application tokens. Missing links, expired tokens without refresh tokens, or missing scopes produce relinking guidance.

Trackmania player operations distinguish `clubs`, `read_favorite`, and `write_favorite`. ManiaPlanet routes distinguish `basic`, `dedicated`, `maps`, `titles`, and `email`; add `email` only when needed. See the providers' [Trackmania documentation](https://doc.trackmania.com/web/web-services/) and [ManiaPlanet OAuth scope documentation](https://doc.maniaplanet.com/web-services/oauth2). Player mutations still require `AllowWrites` and local write confirmation.

The tool list remains stable per endpoint when linking is enabled: configured player operations advertise their requirements to every authenticated caller, and invocation checks the caller's own link. No per-caller tokens enter shared discovery caches.

## Build and test

```powershell
dotnet test --project Tests/ManiaAPI.MCP.Tests/ManiaAPI.MCP.Tests.csproj -f net11.0
dotnet pack Src/ManiaAPI.MCP -c Release
```

The package ID is `ManiaAPI.MCP`; the installed tool command is `maniaapi-mcp`. Tests cover catalog snapshots, generated invokers, scopes, schemas, binary resources, Gbx headers and map writes, game bindings, HTTP/stdio protocol calls, MRTR approval/decline, OAuth consent, caller isolation, encrypted storage, refresh-token concurrency, and the configured tool-list budget. Live tests retain the repository's explicit `[LiveTest]` opt-in.

The source generator targets .NET Standard 2.0 and emits typed invokers. JSON metadata uses ordinary source declarations because generators cannot consume each other's output; error `MCP001` identifies missing registrations. Account-permission overrides come from the existing live tests. Library framework and language settings remain unchanged. The host emits `ManiaAPI.MCP` activities and metrics without logging tool arguments or upstream secret-bearing exception messages.
