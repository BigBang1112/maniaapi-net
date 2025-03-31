# ManiaAPI.Xml.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.Xml.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.Xml.Extensions.Hosting/)

Provides an efficient way to inject all XML services into your application.

## Setup for TMUF

```cs
using ManiaAPI.Xml.Extensions.Hosting;

builder.Services.AddMasterServerTMUF();
```

## Setup for ManiaPlanet

Relying on `Maniaplanet relay 2` to continue running:

```cs
using ManiaAPI.Xml.Extensions.Hosting;

builder.Services.AddMasterServerMP4();
```

You can now inject `MasterServerMP4` (registered as singleton) and use it without additional steps. Compression is enabled.

If you want to be extra sure the correct master server is selected, use this setup:

```cs
using ManiaAPI.Xml.Extensions.Hosting;

builder.Services.AddInitServerMP4();
builder.Services.AddMasterServerMP4();

// In a scope, retrieve init server, and use it for validating the master server
// No need for CreateScopeAsync if your service is scoped
await using var scope = provider.CreateScopeAsync();

var initServer = scope.ServiceProvider.GetRequiredService<InitServerMP4>();
var masterServer = scope.ServiceProvider.GetRequiredService<MasterServerMP4>();

await initServer.ValidateAsync(masterServer);
```

## Setup for TMT

```cs
using ManiaAPI.Xml.Extensions.Hosting;

// Register the services
builder.Services.AddMasterServerTMT();

// Do the setup
// This should run at the start of your application, or when you need to refresh the master servers
// No need for CreateScopeAsync if your service is scoped
await using var scope = provider.CreateScopeAsync();

foreach (var platform in Enum.GetValues<Platform>())
{
    var initServer = scope.ServiceProvider.GetRequiredKeyedService<InitServerTMT>(platform);
    var waitingParams = await initServer.GetWaitingParamsAsync(cancellationToken);
    var masterServer = scope.ServiceProvider.GetRequiredKeyedService<MasterServerTMT>(platform);
    masterServer.Client.BaseAddress = waitingParams.MasterServers.First().GetUri();
}
```

Features this last setup brings:

- **You can inject `AggregatedMasterServerTMT` to conveniently work with all master servers**
- You can inject `ImmutableDictionary<Platform, MasterServerTMT>` to get all master servers as individual instances
- If you don't need specific platform context, you can inject `IEnumerable<MasterServerTMT>` to get all master servers
- Specific `InitServerTMT` and `MasterServerTMT` can be injected using `[FromKeyedServices(Platform.PC)]`
- All `MasterServerTMT` handle compression

> [!WARNING]
> If you just inject `MasterServerTMT` alone, it will give the last-registered one (in this case, PS4). If you need a specific platform, use `[FromKeyedServices(...)]`.
