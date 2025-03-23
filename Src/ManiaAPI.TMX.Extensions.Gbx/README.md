# ManiaAPI.TMX.Extensions.Gbx

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TMX.Extensions.Gbx?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TMX.Extensions.Gbx/)

Connects `ManiaAPI.TMX` with [GBX.NET](https://github.com/BigBang1112/gbx-net) features.

## Features

- Get track Gbx header
- Get track Gbx object
- Get replay Gbx header
- Get replay Gbx object

## Example

```cs
using ManiaAPI.TMX;
using ManiaAPI.TMX.Extensions.Gbx;

// Pick one from TMUF, TMNF, Nations, Sunrise, Original
var tmx = new TMX(TmxSite.TMUF);

// Get the track object
var map = await tmx.GetTrackGbxNodeAsync(12345);

Console.WriteLine("Number of blocks: " + map.GetBlocks().Count());
```