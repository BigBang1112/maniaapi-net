# ManiaAPI.NadeoAPI.Extensions.Gbx

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.NadeoAPI.Extensions.Gbx?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.NadeoAPI.Extensions.Gbx/)

Connects `ManiaAPI.NadeoAPI` with [GBX.NET](https://github.com/BigBang1112/gbx-net) features to provide convenient map upload and **map update**.

## Features

- Upload a map
- Update a map

## Example

A bit more advanced example to show how you can update a map without having to manually specify the map ID:

```cs
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Gbx;
using GBX.NET;
using GBX.NET.Engines.Game;

var ns = new NadeoServices();
await ns.AuthorizeAsync("mylogin", "mypassword", AuthorizationMethod.UbisoftAccount);

// Parse the map Gbx header
var mapFileName = "Path/To/Map.Map.Gbx";
var map = Gbx.ParseHeaderNode<CGameCtnChallenge>(mapFileName);

// Get the map info (we need map ID, not map UID)
var mapInfo = await ns.GetMapInfoAsync(map.MapUid);

// Update the map (no leaderboard lost!)
await ns.UpdateMapAsync(mapInfo.MapId, mapFileName);
```

You can also pass the `CGameCtnChallenge` instance directly, but it is not recommended as the object is re-serialized and some data might change or corrupt (rarely, but still possible).

> This example parses the map twice, which is not optimal. Currently, a method overload that can call `GetMapInfoAsync` behind the scenes is missing.