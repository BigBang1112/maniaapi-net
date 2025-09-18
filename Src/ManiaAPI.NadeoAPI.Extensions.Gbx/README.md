# ManiaAPI.NadeoAPI.Extensions.Gbx

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.NadeoAPI.Extensions.Gbx?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.NadeoAPI.Extensions.Gbx/)

Connects `ManiaAPI.NadeoAPI` with [GBX.NET](https://github.com/BigBang1112/gbx-net) features to provide convenient map upload and **map update**.

## Features

- Upload a map
- Update a map

## Example

A simple example to show how you can update a map without having to manually specify the map ID:

```cs
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Gbx;

var ns = new NadeoServices();
await ns.AuthorizeAsync("mylogin", "mypassword", AuthorizationMethod.UbisoftAccount);

// Update the map (no leaderboard lost!)
await ns.UpdateMapAsync("Path/To/Map.Map.Gbx");
```

Map ID is required to update a map, but you don't have to specify it manually. The extension will check the map UID and fetch the map ID from NadeoServices.

You can also pass the `CGameCtnChallenge` instance directly, but it is not recommended as the object is re-serialized and some data might change or corrupt (rarely, but still possible).