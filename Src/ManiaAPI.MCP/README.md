# ManiaAPI.MCP

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.MCP?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.MCP/)

An MCP server that lets AI assistants talk to Trackmania and ManiaPlanet APIs through ManiaAPI.NET. Ask your assistant about leaderboards, maps, clubs, or players, and it figures out which API to call.

Covers NadeoAPI, Trackmania web API, ManiaPlanet web, Channel Record, and Channel Feedback APIs, Trackmania Web Services, Trackmania Exchange, UnitedLadder, and master servers of TMUF, TMNF, ManiaPlanet, and TMTurbo.

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
claude mcp add maniaapi -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" -- maniaapi-mcp
```

Only want some games? Pick toolsets from `trackmania2020`, `exchange`, `tmuf`, `tmnf`, `maniaplanet`, and `trackmaniaturbo`:

```
claude mcp add maniaapi -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" -- maniaapi-mcp --toolsets tmnf,exchange
```

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

## Credentials

Exchange, master servers, and ManiaPlanet reads work without any credentials. The rest needs them:

- `ManiaAPI:NadeoAPI:Login` and `Password` for TM2020 (Nadeo API), create your service account [here](https://www.trackmania.com/player/service-account)
- `ManiaAPI:TrackmaniaAPI:ClientId` and `ClientSecret` for the Trackmania web API
- `ManiaAPI:TrackmaniaWS:Username` and `Password` for Trackmania Web Services

Pass them as environment variables (`:` becomes `__`), user secrets, or `appsettings.json`. Missing credentials just hide the operations that need them.

## Run over HTTP

Set at least one API key, then:

```
maniaapi-mcp --http --urls http://localhost:5199
```

Clients send the key in the `X-Api-Key` header. Connect to `/mcp` for everything, or to a game endpoint like `/mcp/trackmania2020`, `/mcp/tmnf`, or `/mcp/exchange`.

```json
{
  "ManiaAPI": {
    "Http": { "ApiKeys": ["some-long-random-key"] }
  }
}
```

## Writes

Disabled by default. Set `ManiaAPI:AllowWrites` to `true` to allow things like editing clubs or uploading maps.

Every write asks for your approval first. Writes only work locally (stdio, or HTTP on loopback).

## Link player accounts

Some operations act as a player rather than an app (favorites, player clubs, ManiaPlanet player data). Turn on `ManiaAPI:AccountLinking:Enabled` (it also needs `PublicOrigin`, `StorePath`, and `DataProtectionKeyPath`), then `POST /account-links/trackmania` or `/account-links/maniaplanet` with your API key and open the returned URL to sign in.

## Build from source

```
dotnet user-secrets set ManiaAPI:UserAgent "my-assistant/1.0 (contact: you@example.com)" --project Src/ManiaAPI.MCP
dotnet run --project Src/ManiaAPI.MCP
```
