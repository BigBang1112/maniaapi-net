# ManiaAPI.Dedimania

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.Dedimania?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.Dedimania/)

Wraps the Dedimania records API, the records database of TMF (http://dedimania.net:8000/MAP) and TM2 (http://dedimania.net:8080/MAP) servers.

## Features

- Get a map with its records, optionally only of Rounds or Time Attack

## Setup

```cs
using ManiaAPI.Dedimania;

var dedimania = new Dedimania(DedimaniaGame.TMF, "MyApp/1.0 (contact: me@example.com)");
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.Dedimania;

builder.Services.AddHttpClient(nameof(Dedimania))
    .AddTypedClient(client => new Dedimania(client, DedimaniaGame.TMF));
```

## Usage

```cs
var map = await dedimania.GetMapAsync("BeySZdnfuSh4nHY5xztiXLmlrXe"); // A01-Race (TMNF)

if (map is not null)
{
    foreach (var record in map.Records)
    {
        Console.WriteLine($"{record.Time}: {record.Login} ({record.Mode})");
    }
}
```

`GetMapAsync` returns null when Dedimania doesn't know the map, and at most 100 records. Use `DedimaniaGame.TMF` for TMUF and TMNF maps, and `DedimaniaGame.TM2` for TM2 maps. Pass `GameMode.Rounds` or `GameMode.TimeAttack` to get records of one mode only.
