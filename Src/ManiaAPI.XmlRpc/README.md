# ManiaAPI.XmlRpc

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.XmlRpc?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.XmlRpc/)

Wraps TMF, TMT, and ManiaPlanet XML-RPC ingame APIs. **Does not include dedicated server XML-RPC.**

It currently **does not support any authentication** for its complexity and security reasons. If some of the leaderboard methods will become secured with authentication though, this will be considered. For authenticated functionality in TMUF, use the [TMF.NET](https://github.com/Laiteux/TMF.NET) library.

For dedicated server XML-RPC, use the [GbxRemote.Net](https://github.com/EvoEsports/GbxRemote.Net) library.

## Features

For TMUF:

- Get scores
  - Top 10 leaderboards
  - All records (without identities)
  - Skillpoints
  - Medals
- Get ladder zone rankings
- Get ladder player rankings

For ManiaPlanet:

- Get campaign and map leaderboard from multiple campaigns/maps at once
  - Top 10 leaderboards
  - All records (without identities)
  - Skillpoints
  - Medals
- Get campaign and map leaderboards
 - **Any range of records**
 - Skillpoints
 - Medals
- Get available master servers

For TMT:

- Get all map records (without identities)
- Get campaign medal rankings (without identities)
- Get available master servers

For all games:

- Get all available zones

## Setup for TMUF

```cs
using ManiaAPI.XmlRpc;

var masterServer = new MasterServerTMUF();
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.XmlRpc;

builder.Services.AddHttpClient<MasterServerTMUF>(client => client.BaseAddress = new Uri(MasterServerTMUF.DefaultAddress));
```

## Setup for ManiaPlanet

First examples assume `Maniaplanet relay 2` master server is still running.

```cs
using ManiaAPI.XmlRpc;

var masterServer = new MasterServerMP4();
```

Because the responses can be quite large sometimes, it's **recommended to accept compression** on the client.

```cs
using ManiaAPI.XmlRpc;

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
{
    BaseAddress = new System.Uri(MasterServerMP4.DefaultAddress)
};
var masterServer = new MasterServerMP4(httpClient);
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.XmlRpc;

builder.Services.AddHttpClient<MasterServerMP4>(client => client.BaseAddress = new Uri(MasterServerMP4.DefaultAddress))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip
    });
```

In case `Maniaplanet relay 2` shuts down / errors out, you have to reach out to init server with `GetWaitingParams` and retrieve an available relay. That's how the game client does it (thanks Mystixor for figuring this out).

To be most inline with the game client, you should validate the master server first with `ValidateAsync`. Behind the scenes, it first requests `GetApplicationConfig`, then on catched HTTP exception, it requests `GetWaitingParams` from the init server and use the available master server instead.

```cs
using ManiaAPI.XmlRpc;

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
{
    BaseAddress = new System.Uri(MasterServerMP4.DefaultAddress)
};
var masterServer = new MasterServerMP4(httpClient);

await masterServer.ValidateAsync(); // Do this for reliability

// The master server is now ready to use
```

With DI, it is recommended to separate the init server's `HttpClient` from the master server. Note how `MasterServerMP4` is registered as a singleton, so that it remembers the address.

You don't have to enable decompression for the init server, as it does not return large responses.

```cs
using ManiaAPI.XmlRpc;

// Register the services
builder.Services.AddHttpClient<InitServerMP4>(client => client.BaseAddress = new Uri(InitServerMP4.DefaultAddress));
builder.Services.AddHttpClient<MasterServerMP4>(client => client.BaseAddress = new Uri(MasterServerMP4.DefaultAddress))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip
    });
builder.Services.AddSingleton<MasterServerMP4>();

// Do the setup
// This should run at the start of your application, or when you need to refresh the master servers
await using var scope = provider.CreateScopeAsync();

var initServer = scope.ServiceProvider.GetRequiredService<InitServerMP4>();
var masterServer = scope.ServiceProvider.GetRequiredService<MasterServerMP4>();

await masterServer.ValidateAsync(initServer);

// The master server is now ready to use
```

## Setup for TMT

TMT handles 3 platforms: PC, XB1, and PS4. Each have their own init server and master server. Nadeo still tends to change these master servers, so it's recommended to first go through the init server.

```cs
using ManiaAPI.XmlRpc;

var initServer = new InitServerTMT(Platform.PC);
var waitingParams = await initServer.GetWaitingParamsAsync();

var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

// You can repeat this exact setup for XB1 and PS4 as well if you want to work with those platforms, with something like Dictionary<Platform, MasterServerTMT> ...
```

Because the responses can be quite large sometimes, it's **recommended to accept compression** on the client for the **master server**. Init server does not return large responses, so it's not necessary for that one.

```cs
using ManiaAPI.XmlRpc;

var initServer = new InitServerTMT(Platform.PC);
var waitingParams = await initServer.GetWaitingParamsAsync();

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
{
    BaseAddress = waitingParams.MasterServers.First().GetUri()
};
var masterServer = new MasterServerTMT(httpClient);

// You can repeat this exact setup for XB1 and PS4 as well if you want to work with those platforms, with something like Dictionary<Platform, MasterServerTMT> ...
```

or with DI, using an injected `HttpClient` (not viable for multiple platforms). Note how `MasterServerTMT` is registered as a singleton, so that it remembers the address.

```cs
using ManiaAPI.XmlRpc;

// Register the services
builder.Services.AddHttpClient<InitServerTMT>(client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(Platform.PC)));
builder.Services.AddHttpClient<MasterServerTMT>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip
    });
builder.Services.AddSingleton<MasterServerTMT>();

// Do the setup
// This should run at the start of your application, or when you need to refresh the master servers
await using var scope = provider.CreateScopeAsync();

var initServer = scope.ServiceProvider.GetRequiredService<InitServerTMT>();
var waitingParams = await initServer.GetWaitingParamsAsync();

var masterServer = scope.ServiceProvider.GetRequiredService<MasterServerTMT>();
masterServer.Client.BaseAddress = waitingParams.MasterServers.First().GetUri();
```

For DI setup with multiple platforms, you can use keyed services:

```cs
using ManiaAPI.XmlRpc;

// Register the services
foreach (var platform in Enum.GetValues<Platform>())
{
    builder.Services.AddHttpClient($"{nameof(InitServerTMT)}_{platform}", client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(platform)));
    builder.Services.AddHttpClient($"{nameof(MasterServerTMT)}_{platform}")
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });

    builder.Services.AddKeyedScoped(platform, (provider, key) => new InitServerTMT(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(InitServerTMT)}_{key}")));

    builder.Services.AddKeyedSingleton(platform, (provider, key) => new MasterServerTMT(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(MasterServerTMT)}_{key}")));
    builder.Services.AddSingleton(provider => provider.GetRequiredKeyedService<MasterServerTMT>(platform));
}

builder.Services.AddSingleton(provider => Enum.GetValues<Platform>()
    .ToImmutableDictionary(platform => platform, platform => provider.GetRequiredKeyedService<MasterServerTMT>(platform)));

// Do the setup
// This should run at the start of your application, or when you need to refresh the master servers
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

- You can inject `ImmutableDictionary<Platform, MasterServerTMT>` to get all master servers
- If you don't need specific platform context, you can inject `IEnumerable<MasterServerTMT>` to get all master servers
- Specific `InitServerTMT` and `MasterServerTMT` can be injected using `[FromKeyedServices(Platform.PC)]`

> [!WARNING]
> If you just inject `MasterServerTMT` alone, it will give the last-registered one (in this case, PS4). If you need a specific platform, use `[FromKeyedServices(...)]`.