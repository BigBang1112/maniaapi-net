# ManiaAPI.XmlRpc

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.XmlRpc?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.XmlRpc/)

Wraps TMF, TMT, and ManiaPlanet XML-RPC ingame APIs. **Does not include dedicated server XML-RPC.**

It currently **does not support any authentication** for its complexity and security reasons. If some of the leaderboard methods will become secured with authentication though, this will be considered. For authenticated functionality in TMUF, use the [TMF.NET](https://github.com/Laiteux/TMF.NET) library.

For dedicated server XML-RPC, use the [GbxRemote.Net](https://github.com/EvoEsports/GbxRemote.Net) library.

## Possibilities

For TMUF:

- Get scores
  - Top 10 leaderboards
  - All records (without identities)
  - Skillpoints
  - Medals
- Get ladder zone rankings
- Get ladder player rankings
- Get available master servers

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

- TODO

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

builder.Services.AddScoped<MasterServerTMUF>();
builder.Services.AddHttpClient<MasterServerTMUF>(client => client.BaseAddress = new(MasterServerTMUF.DefaultAddress));
```

## Setup for ManiaPlanet

First examples assume `Maniaplanet relay 2` master server is still running.

```cs
using ManiaAPI.XmlRpc;

var masterServer = new MasterServerMP4();
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.XmlRpc;

builder.Services.AddScoped<MasterServerMP4>();
builder.Services.AddHttpClient<MasterServerMP4>(client => client.BaseAddress = new Uri(MasterServerMP4.DefaultAddress));
```

In case `Maniaplanet relay 2` shuts down / errors out, you have to reach out to init server with `GetWaitingParams` and retrieve an available relay. That's how the game client does it (thanks Mystixor for figuring this out).

To be most inline with the game client, you should validate the master server first with `ValidateAsync`. Behind the scenes, it first requests `GetApplicationConfig`, then on catched HTTP exception, it requests `GetWaitingParams` from the init server and use the available master server instead.

```cs
using ManiaAPI.XmlRpc;

var masterServer = new MasterServerMP4();

await masterServer.ValidateAsync(); // Do this for reliability

// The master server is now ready to use
```

with DI, it is recommended to separate the init server's `HttpClient` from the master server.

```cs
using ManiaAPI.XmlRpc;

// Register the services
builder.Services.AddScoped<InitServerMP4>();
builder.Services.AddScoped<MasterServerMP4>();
builder.Services.AddHttpClient<InitServerMP4>(client => client.BaseAddress = new Uri(InitServerMP4.DefaultAddress));
builder.Services.AddHttpClient<MasterServerMP4>(client => client.BaseAddress = new Uri(MasterServerMP4.DefaultAddress));

// Do the setup
var initServer = provider.GetRequiredService<InitServerMP4>();
var masterServer = provider.GetRequiredService<MasterServerMP4>();

await masterServer.ValidateAsync(initServer); // Do this for reliability

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

or with DI, using an injected `HttpClient` (not viable for multiple platforms):

```cs
using ManiaAPI.XmlRpc;

// Register the services
builder.Services.AddScoped<InitServerTMT>();
builder.Services.AddScoped<MasterServerTMT>();
builder.Services.AddHttpClient<InitServerTMT>(client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(Platform.PC)));
builder.Services.AddHttpClient<MasterServerTMT>();

// Do the setup
var initServer = provider.GetRequiredService<InitServerTMT>();
var waitingParams = await initServer.GetWaitingParamsAsync();

var masterServer = provider.GetRequiredService<MasterServerTMT>();
masterServer.Client.BaseAddress = waitingParams.MasterServers.First().GetUri();
```

For DI setup with multiple platforms, you can use keyed services:

```cs
using ManiaAPI.XmlRpc;

// Register the services
foreach (Platform platform in Enum.GetValues(typeof(Platform)))
{
    builder.Services.AddKeyedScoped<InitServerTMT>(platform, (provider, key) => new InitServerTMT(provider.GetRequiredService<IHttpClientFactory>().CreateClient(platform.ToString())));
    builder.Services.AddKeyedScoped<MasterServerTMT>(platform, (provider, key) => new MasterServerTMT(provider.GetRequiredService<IHttpClientFactory>().CreateClient(platform.ToString())));
    builder.Services.AddHttpClient<InitServerTMT>(platform.ToString(), client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(platform)));
    builder.Services.AddHttpClient<MasterServerTMT>(platform.ToString());
}

// Do the setup
foreach (Platform platform in Enum.GetValues(typeof(Platform)))
{
    var initServer = provider.GetRequiredService<InitServerTMT>(platform);
    var waitingParams = await initServer.GetWaitingParamsAsync();
    var masterServer = provider.GetRequiredService<MasterServerTMT>(platform);
    masterServer.Client.BaseAddress = waitingParams.MasterServers.First().GetUri();
}
```