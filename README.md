# ManiaAPI.NET

A wrapper for these web APIs:

- Nadeo API (official TM2020 ingame API)
- Trackmania web API
- ManiaPlanet web API
- Trackmania.io
- Trackmania Exchange
- XML-RPC (for TMF and ManiaPlanet)

This set of libraries was made to be very easy and straightforward to use, but also easily mocked, so that it can be integrated into the real world in no time.

## ManiaAPI.NadeoAPI

TBD

### ManiaAPI.NadeoAPI.Extensions.Gbx

TBD

## ManiaAPI.TrackmaniaAPI

TBD

## ManiaAPI.ManiaPlanetAPI

TBD

### ManiaAPI.ManiaPlanetAPI.Extensions.Hosting

TBD

## ManiaAPI.TrackmaniaIO

TBD

## ManiaAPI.TMX

Wraps https://tm-exchange.com/ (old TMX).

#### Setup

```cs
using ManiaAPI.TMX;

// Pick one from TMUF, TMNF, Nations, Sunrise, Original
var tmx = new TMX(TmxSite.TMUF);
```

#### Possibilities

- Get replays
- Search leaderboards
- Search trackpacks
- Search tracks
- Search users
- Get Gbx URLs and HTTP responses
- Get image URLs and HTTP responses

### ManiaAPI.TMX.Extensions.Gbx

TBD

## ManiaAPI.XmlRpc

Wraps TMF, TMT, and ManiaPlanet XML-RPC ingame APIs. **Does not include dedicated server XML-RPC.**

#### Setup for TMUF

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

#### Setup for ManiaPlanet

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

in case `Maniaplanet relay 2` shuts down, you have to reach out to `InitServerMP4` with `GetWaitingParams` and retrieve an available relay. That's how the game client does it (thanks Mystixor for figuring this out).

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

#### Setup for TMT

TMT handles 3 platforms: PC, XB1, and PS4. Each have their own init server and master server. Nadeo still tends to change the master servers, so it's recommended to first go through the init server.

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
builder.Services.AddKeyedScoped<InitServerTMT>(Platform.PC, (provider, key) => new InitServerTMT(provider.GetRequiredService<IHttpClientFactory>().CreateClient("PC")));
builder.Services.AddKeyedScoped<InitServerTMT>(Platform.XB1, (provider, key) => new InitServerTMT(provider.GetRequiredService<IHttpClientFactory>().CreateClient("XB1")));
builder.Services.AddKeyedScoped<InitServerTMT>(Platform.PS4), (provider, key) => new InitServerTMT(provider.GetRequiredService<IHttpClientFactory>().CreateClient("PS4")));
builder.Services.AddKeyedScoped<MasterServerTMT>(Platform.PC);
builder.Services.AddKeyedScoped<MasterServerTMT>(Platform.XB1);
builder.Services.AddKeyedScoped<MasterServerTMT>(Platform.PS4);
builder.Services.AddHttpClient<InitServerTMT>("PC", client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(Platform.PC)));
builder.Services.AddHttpClient<InitServerTMT>("XB1", client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(Platform.XB1)));
builder.Services.AddHttpClient<InitServerTMT>("PS4", client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(Platform.PS4)));
builder.Services.AddHttpClient<MasterServerTMT>("PC");
builder.Services.AddHttpClient<MasterServerTMT>("XB1");
builder.Services.AddHttpClient<MasterServerTMT>("PS4");

// Do the setup
var initServerPC = provider.GetRequiredService<InitServerTMT>(Platform.PC);
var waitingParamsPC = await initServerPC.GetWaitingParamsAsync();
var masterServerPC = provider.GetRequiredService<MasterServerTMT>(Platform.PC);
masterServerPC.Client.BaseAddress = waitingParamsPC.MasterServers.First().GetUri();

var initServerXB1 = provider.GetRequiredService<InitServerTMT>(Platform.XB1);
var waitingParamsXB1 = await initServerXB1.GetWaitingParamsAsync();
var masterServerXB1 = provider.GetRequiredService<MasterServerTMT>(Platform.XB1);
masterServerXB1.Client.BaseAddress = waitingParamsXB1.MasterServers.First().GetUri();

var initServerPS4 = provider.GetRequiredService<InitServerTMT>(Platform.PS4);
var waitingParamsPS4 = await initServerPS4.GetWaitingParamsAsync();
var masterServerPS4 = provider.GetRequiredService<MasterServerTMT>(Platform.PS4);
masterServerPS4.Client.BaseAddress = waitingParamsPS4.MasterServers.First().GetUri();
```