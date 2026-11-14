# ManiaAPI.MCP

An MCP server that lets AI assistants talk to Trackmania and ManiaPlanet APIs through [ManiaAPI.NET](https://github.com/BigBang1112/maniaapi-net). Ask your assistant about leaderboards, maps, clubs, or players, and it figures out which API to call.

Covers NadeoAPI, Trackmania web API, ManiaPlanet web, Channel Record, and Channel Feedback APIs, Trackmania Web Services, Trackmania Exchange and Item Exchange, UnitedLadder, and master servers of TMUF, TMNF, ManiaPlanet, and TMTurbo.

## Tags

- `latest`, `alpine`: the current build of the `main` branch, on Alpine
- `<version>`, `<version>-alpine`: a specific MCP version, like `0.1.0`

Images are built for `linux/amd64` and `linux/arm64`, and rebuilt weekly to pick up base image updates.

## Run over HTTP

The image serves stateless Streamable HTTP on port 8080 by default, as a non-root user. It needs a User-Agent that identifies your deployment to the upstream services, and at least one API key:

```
docker run -d -p 8080:8080 \
  -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" \
  -e ManiaAPI__Http__ApiKeys__0="some-long-random-key" \
  bigbang1112/maniaapi-mcp
```

Clients send the key in the `X-Api-Key` header. Connect to `/mcp` for everything, or to a game endpoint like `/mcp/trackmania2020`, `/mcp/tmnf`, or `/mcp/exchange`. `/healthz` needs no key and is used by the image's health check.

Each API key can make 120 MCP requests per minute; past that, requests get `429 Too Many Requests` until the minute is over.

## Run over stdio

Run it interactively, with any arguments other than `--http`, like `--toolsets tmnf,exchange`, or `""` for none:

```
# Claude Code
claude mcp add maniaapi -- docker run -i --rm \
  -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" \
  bigbang1112/maniaapi-mcp ""

# Codex
codex mcp add maniaapi -- docker run -i --rm \
  -e ManiaAPI__UserAgent="my-assistant/1.0 (contact: you@example.com)" \
  bigbang1112/maniaapi-mcp ""
```

## Credentials

Exchange, master servers, and ManiaPlanet reads work without any credentials. The rest needs them:

- `ManiaAPI__NadeoAPI__Login` and `ManiaAPI__NadeoAPI__Password` for TM2020 (Nadeo API), from a [service account](https://www.trackmania.com/player/service-account)
- `ManiaAPI__TrackmaniaAPI__ClientId` and `ManiaAPI__TrackmaniaAPI__ClientSecret` for the Trackmania web API, and to link Trackmania accounts
- `ManiaAPI__ManiaPlanet__ClientId` and `ManiaAPI__ManiaPlanet__ClientSecret` to link ManiaPlanet accounts; optional for the ManiaPlanet web API, which also works without them
- `ManiaAPI__TrackmaniaWS__Username` and `ManiaAPI__TrackmaniaWS__Password` for Trackmania Web Services

Pass them as environment variables, or as [Docker secrets](https://docs.docker.com/engine/swarm/secrets/) mounted in `/run/secrets`, one file per setting named like the variable. Missing credentials just hide the operations that need them.

## Writes

Writes (club management, favorites, map upload, ...) are off by default and always ask for confirmation. Over HTTP they only work when the server listens on loopback, so this image refuses to start with them over HTTP. To use them, set `ManiaAPI__AllowWrites=true` and run over stdio.

## More

See the [full documentation](https://github.com/BigBang1112/maniaapi-net/tree/main/Src/ManiaAPI.MCP) for toolsets, player account linking, and the other settings.
