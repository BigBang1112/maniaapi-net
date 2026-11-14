# ManiaAPI.MCP

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.MCP?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.MCP/)

An MCP server that lets AI assistants talk to Trackmania and ManiaPlanet APIs through ManiaAPI.NET. Ask your assistant about leaderboards, maps, clubs, or players, and it figures out which API to call.

Covers NadeoAPI, Trackmania web API, ManiaPlanet web, Channel Record, and Channel Feedback APIs, Trackmania Web Services, Trackmania Exchange and Item Exchange, UnitedLadder, and master servers of TMUF, TMNF, ManiaPlanet, and TMTurbo.

### Features

- Over 350 operations reachable through 3 small gateway tools (search, describe, invoke), so your tool list stays lean
- Ready-made tools for common questions
- Game-scoped endpoints, so an assistant for TMNF doesn't see TM2020 stuff
- Map, replay, and image downloads as MCP resources, with Gbx header summaries
- Optional writes (club management, favorites, map upload, ...) that always ask for confirmation
- Linked player accounts via Trackmania/ManiaPlanet OAuth
- Stdio and stateless Streamable HTTP

## Install

Make sure you have the .NET 11 SDK available.

```
dotnet tool install -g ManiaAPI.MCP
```

Then run it with `maniaapi-mcp`. Stdio is the default, and all games are enabled.

## Setup

`ManiaAPI:UserAgent` is required: set it to something that identifies you, like `my-assistant/1.0 (contact: you@example.com)`. It's sent to every upstream service, followed by the library's own User-Agent, so the people running those services can tell who's calling. The server won't start without it.

```
# Claude Code
claude mcp add maniaapi -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" -- maniaapi-mcp

# Codex
codex mcp add maniaapi --env ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" -- maniaapi-mcp
```

Only want some games? Pick toolsets from `trackmania2020`, `exchange`, `tmuf`, `tmnf`, `maniaplanet`, and `trackmaniaturbo`:

```
# Claude Code
claude mcp add maniaapi -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" -- maniaapi-mcp --toolsets tmnf,exchange

# Codex
codex mcp add maniaapi --env ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" -- maniaapi-mcp --toolsets tmnf,exchange
```

The `exchange` toolset includes Item Exchange for TM2020 and ManiaPlanet. Set `ManiaAPI:Exchange:ItemExchange` to `false` to leave it out.

Most clients accept a JSON config like this:

```json
{
  "mcpServers": {
    "maniaapi": {
      "command": "maniaapi-mcp",
      "env": {
        "ManiaAPI__UserAgent": "my-assistant/1.0 (contact: you@example.com)",
        "ManiaAPI__NadeoAPI__Login": "service_name",
        "ManiaAPI__NadeoAPI__Password": "ls>97jO>e3>>D/Ce"
      }
    }
  }
}
```

Codex uses TOML instead, in `~/.codex/config.toml`:

```toml
[mcp_servers.maniaapi]
command = "maniaapi-mcp"

[mcp_servers.maniaapi.env]
ManiaAPI__UserAgent = "my-assistant/1.0 (contact: you@example.com)"
ManiaAPI__NadeoAPI__Login = "service_name"
ManiaAPI__NadeoAPI__Password = "ls>97jO>e3>>D/Ce"
```

## Credentials

Exchange, master servers, and ManiaPlanet reads work without any credentials. The rest needs them:

- `ManiaAPI:NadeoAPI:Login` and `Password` for TM2020 (Nadeo API), create your service account [here](https://www.trackmania.com/player/service-account)
- `ManiaAPI:TrackmaniaAPI:ClientId` and `ClientSecret` for the Trackmania web API, and to link Trackmania accounts
- `ManiaAPI:ManiaPlanet:ClientId` and `ClientSecret` to link ManiaPlanet accounts; optional for the ManiaPlanet web API, which also works without them
- `ManiaAPI:TrackmaniaWS:Username` and `Password` for Trackmania Web Services

Pass them as environment variables (`:` becomes `__`), user secrets, or `appsettings.json`. Missing credentials just hide the operations that need them.

## Run over HTTP

Set at least one API key, then:

```
maniaapi-mcp --http --urls http://localhost:5199
```

Clients send the key in the `X-Api-Key` header. Connect to `/mcp` for everything, or to a game endpoint like `/mcp/trackmania2020`, `/mcp/tmnf`, or `/mcp/exchange`.

Each API key can make 120 MCP requests per minute; past that, requests get `429 Too Many Requests` until the minute is over.

```json
{
  "ManiaAPI": {
    "Http": { "ApiKeys": ["some-long-random-key"] }
  }
}
```

## Run with Docker

Build the Alpine image from the repository root:

```
docker build -f Src/ManiaAPI.MCP/Dockerfile -t maniaapi-mcp .
```

It runs over HTTP on port 8080 by default, as a non-root user:

```
docker run -d -p 8080:8080 -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" -e ManiaAPI__Http__ApiKeys__0="some-long-random-key" maniaapi-mcp
```

`/healthz` needs no API key and is used by the image's health check. Credentials go in environment variables as above, or in [Docker secrets](https://docs.docker.com/engine/swarm/secrets/) mounted in `/run/secrets`, one file per setting named like `ManiaAPI__NadeoAPI__Password`.

For stdio, run it interactively and pass any arguments without `--http`, like `--toolsets tmnf,exchange`, or `""` for none:

```
# Claude Code
claude mcp add maniaapi -- docker run -i --rm -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" maniaapi-mcp ""

# Codex
codex mcp add maniaapi -- docker run -i --rm -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" maniaapi-mcp ""
```

Writes stay off in the container over HTTP, as it listens on all interfaces rather than loopback.

## Writes

Disabled by default. Set `ManiaAPI:AllowWrites` to `true` to allow things like editing clubs or uploading maps.

Every write asks for your approval first. Writes only work locally (stdio, or HTTP on loopback).

The server asks for approval with a prompt in your MCP client, which needs a client that supports MRTR. Other clients can't show it, so their writes are refused. To allow writes from them anyway, set `ManiaAPI:TrustClientApproval` to `true`. The assistant then approves a write by passing `confirm: true`, so the only check left is your client's own tool approval: make sure it asks you before each call.

The approval comes back in a second request, which has to reach the same server instance within 5 minutes. Running several instances on the same machine behind a proxy, or restarting in between, can make the approval fail, so run a single instance when writes are on.

## Link player accounts

Some operations act as a player rather than an app (favorites, player clubs, ManiaPlanet player data). Turn on `ManiaAPI:AccountLinking:Enabled` (it also needs `PublicOrigin`, `StorePath`, and `DataProtectionKeyPath`), then `POST /account-links/trackmania` or `/account-links/maniaplanet` with your API key and open the returned URL to sign in.

## Build from source

```
dotnet user-secrets set ManiaAPI:UserAgent "my-assistant/1.0 (contact: you@example.com)" --project Src/ManiaAPI.MCP
dotnet run --project Src/ManiaAPI.MCP
```
