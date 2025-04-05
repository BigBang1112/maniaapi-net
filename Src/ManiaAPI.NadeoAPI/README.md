# ManiaAPI.NadeoAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.NadeoAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.NadeoAPI/)

Wraps the official Nadeo API used in the latest Trackmania (2020). **This API requires authentication.**

After initial authentication, the connectivity is managed by the library, so you don't have to worry about refreshing the token.

The game provides 3 domains, and they are split into 3 separate services:

- `NadeoServices` for the core functionality
- `NadeoLiveServices` for leaderboards, clubs, and other live content
- `NadeoMeetServices` for getting the current Cup of the Day

## Features

For `NadeoServices`:

- Get map records
- Get account records
- Get player zones
- Get API routes
- Get all available zones
- Get player club tags
- Get map info

For `NadeoLiveServices`:

- **Edit club campaigns**
- **Edit club activities**
- Get map info
- Get map leaderboards
- Get map medal records
- Get seasonal campaigns
- Get weekly shorts
- Get TOTDs
- Get club campaigns
- Get club info
- Get club members
- Get club activities
- Get club rooms
- Get player season rankings
- Get active advertisements
- Join daily channel (COTD)

For `NadeoMeetServices`:

- Get the current Cup of the Day

## Setup for a single service

```cs
using ManiaAPI.NadeoAPI;

var ns = new NadeoServices();

await ns.AuthorizeAsync("mylogin", "mypassword", AuthorizationMethod.UbisoftAccount);

// Ready to use
var zones = await ns.GetZonesAsync();
```

You can also use a dedicated server. Just be aware it has some limitations.

```cs
await ns.AuthorizeAsync("my_dedicated_server", "ls>97jO>e3>>D/Ce", AuthorizationMethod.DedicatedServer);
```

For other services, just replace `NadeoServices` with `NadeoLiveServices` or `NadeoMeetServices`.

## Setup for multiple services

```cs
using ManiaAPI.NadeoAPI;

var login = "mylogin";
var password = "mypassword";

var ns = new NadeoServices();
await ns.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);

var nls = new NadeoLiveServices();
await nls.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);

// Ready to use combined

// With NadeoLiveServices
var weeklyCampaigns = await nls.GetSeasonalCampaignsAsync(1);
var campaignMap = weeklyCampaigns.CampaignList.First().Playlist.First();
var mapInfo = await nls.GetMapInfoAsync(campaignMap.MapUid);
var mapLeaderboard = await nls.GetTopLeaderboardAsync(campaignMap.MapUid);

// With NadeoServices
var records = await ns.GetMapRecordsAsync(mapLeaderboard.Top.Top.Select(x => x.AccountId), mapInfo.MapId);
```

For DI, consider using the `ManiaAPI.NadeoAPI.Extensions.Hosting` package. It also handles the authorization for you without additional startup code.