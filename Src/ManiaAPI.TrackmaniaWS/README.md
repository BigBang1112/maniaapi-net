# ManiaAPI.TrackmaniaWS

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaWS?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaWS/)

Wraps https://ws.trackmania.com/ (old TMF web API). **This API requires authorization (via constructor).**

## Features

- Players
  - Info (registration ID from login for example)
  - Nickname as HTML
  - Tags
  - Multiplayer (ladder) and solo (skill points) ranking, with the rank in each zone
- Zones
  - By ID or path, ID from path
  - All zones and child zones, with paging and sorting
- Rankings
  - Solo leaderboard of players, overall or on a challenge (map), in any zone
  - Multiplayer leaderboard of players in any zone and environment
  - Multiplayer leaderboard of zones
- Number of registered accounts
- Short manialink codes
- ManiaHome notifications from a manialink
- Check that the service is up, without needing credentials

Errors throw `TrackmaniaWSResponseException` (an `HttpRequestException`) with the message of the response, like `Invalid environment "Foo"`.

## Setup

```cs
using ManiaAPI.TrackmaniaWS;

var ws = new TrackmaniaWS("tmf_yourapp", "password");

// Ready to use
```

To check that the service is up, `PingAsync` uses its public test route, which ignores the credentials:

```cs
bool up = await ws.PingAsync();
```

For DI, consider using the `ManiaAPI.TrackmaniaWS.Extensions.Hosting` package.