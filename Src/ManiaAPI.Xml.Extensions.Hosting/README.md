# ManiaAPI.Xml.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.Xml.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.Xml.Extensions.Hosting/)

Provides an efficient way to inject all XML services into your application.

## Setup for TMUF

```cs
using ManiaAPI.Xml.Extensions.Hosting;

builder.Services.AddMasterServerTMUF();
```

## Setup for ManiaPlanet

```cs
using ManiaAPI.Xml.Extensions.Hosting;

builder.Services.AddMasterServerMP4();
```

You can now inject `MasterServerMP4`, as long as you're fine relying on `Maniaplanet relay 2` to continue running, and use it without additional steps. Compression is enabled.

If you want to have better control over the selection of master servers, use this setup:

```cs
using ManiaAPI.Xml.Extensions.Hosting;

builder.Services.AddMasterServerMP4();

// --------------------
// Do the setup
var factory = provider.GetRequiredService<IMasterServerMP4Factory>();

// RequestWaitingParamsAsync should run at the start of your application, and when you need to refresh the master servers
var waitingParams = await factory.RequestWaitingParamsAsync();

// For example selects the first available master server (more overloads are available)
var masterServer = factory.CreateClient();
```

Features this setup brings:
- You can inject `IMasterServerMP4Factory` to create multiple instances of `MasterServerMP4` with different master servers and refresh them
- You can inject `MasterServerMP4` to get a default instance using `Maniaplanet relay 2`
- You can inject `InitServerMP4` to get the init server
- All `MasterServerMP4` handle GZIP compression

## Setup for TMT

```cs
using ManiaAPI.Xml.Extensions.Hosting;

// Register the services
builder.Services.AddMasterServerTMT();

// --------------------
// Do the setup
var factory = provider.GetRequiredService<IMasterServerTMTFactory>();

// RequestWaitingParamsAsync should run at the start of your application, and when you need to refresh the master servers
await factory.RequestWaitingParamsAsync();

// For example selects the PC master server
var masterServerPC = factory.CreateClient(Platform.PC);
```

Features this last setup brings:

- **You can inject `AggregatedMasterServerTMT` to conveniently work with all master servers**
- You can inject `IMasterServerTMTFactory` to create multiple instances of `MasterServerTMT` with different master servers
- You can inject `ImmutableDictionary<Platform, MasterServerTMT>` to get all master servers as individual instances
- If you don't need specific platform context, you can inject `IEnumerable<MasterServerTMT>` to get all master servers
- Specific `InitServerTMT` and `MasterServerTMT` can be injected using `[FromKeyedServices(Platform.PC)]`
- All `MasterServerTMT` handle GZIP compression

> [!WARNING]
> If you just inject `MasterServerTMT` alone, it will give the last-registered one (in this case, PS4). If you need a specific platform, use `[FromKeyedServices(...)]`.
