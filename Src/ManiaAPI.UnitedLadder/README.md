# ManiaAPI.UnitedLadder

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.UnitedLadder?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.UnitedLadder/)

Wraps the [UnitedLadder API](https://api.ul.unitedascenders.xyz/docs), which combines TrackMania Forever profiles, rankings, statistics, and historical data. Authentication is not required.

## Features

- Player profiles, including solo and multiplayer statistics, zone ranks, tags, and metadata
- Force a profile refresh through TMF Web Services with `forceWebServices: true`
- Solo and multiplayer rankings by zone and environment
- Track leaderboards, including record scores and ghost links
- A player's official map results and statistics by environment and campaign
- Login registration and United account status
- Player history and progression of skill points, ladder points, or nicknames
- Recent leaderboard snapshots
- Ghost replay downloads by track UID and numeric player ID

## Setup

```cs
using ManiaAPI.UnitedLadder;

using var ul = new UnitedLadder("MyApp/1.0 (contact: me@example.com)");

var player = await ul.GetPlayerAsync("bigbang1112");
var refreshed = await ul.GetPlayerAsync("bigbang1112", forceWebServices: true);
var solo = await ul.GetSoloRankingsAsync(zone: "World|France", page: 0);
var multiplayer = await ul.GetMultiplayerRankingsAsync(environment: "Stadium");
var leaderboard = await ul.GetTrackLeaderboardAsync("BeySZdnfuSh4nHY5xztiXLmlrXe");
var maps = await ul.GetPlayerMapsAsync("bigbang1112");
var breakdown = await ul.GetPlayerSoloBreakdownAsync("bigbang1112");
var registration = await ul.CheckLoginAsync("bigbang1112");
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.UnitedLadder;

builder.Services.AddHttpClient<UnitedLadder>();
```

All paged methods use zero-based pages. Zone paths such as `World|France` and player logins are escaped automatically. Environments are `Stadium`, `Island`, `Desert`, `Rally`, `Bay`, `Coast`, `Snow`, `Mixed`, or `Merge`; multiplayer rankings default to `Merge`.

## History and progression

```cs
var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
var history = await ul.GetPlayerHistoryAsync("bigbang1112", startDate: start);
var points = await ul.GetPlayerProgressionAsync("bigbang1112",
    field: "ladderpoints", environment: "Stadium", startDate: start);
var names = await ul.GetPlayerProgressionAsync("bigbang1112", field: "nickname");
var snapshot = await ul.GetLeaderboardSnapshotAsync("solo", environment: "solo", page: 0);
```

## Replay files

```cs
using var response = await ul.GetReplayResponseAsync("BeySZdnfuSh4nHY5xztiXLmlrXe", player.Id);
await using var file = File.Create("ghost.Replay.Gbx");
await response.Content.CopyToAsync(file);
```
