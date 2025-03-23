# ManiaAPI.NET

[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/BigBang1112/maniaapi-net?include_prereleases&style=for-the-badge&logo=github)](https://github.com/BigBang1112/maniaapi-net/releases)

A wrapper for these web APIs:

- Nadeo API (official TM2020 ingame API)
- Trackmania web API
- ManiaPlanet web API
- Trackmania.io
- Trackmania Exchange
- XML-RPC (for TMF and ManiaPlanet)

This set of libraries was made to be very easy and straightforward to use, but also easily mocked, so that it can be integrated into the real world in no time.

## Packages

Anything you can imagine!

- [ManiaAPI.NadeoAPI](#maniaapinadeoapi)
- [ManiaAPI.NadeoAPI.Extensions.Gbx](#maniaapinadeoapiextensionsgbx)
- [ManiaAPI.TrackmaniaAPI](#maniaapitrackmaniaapi)
- [ManiaAPI.TrackmaniaAPI.Extensions.Hosting](#maniaapitrackmaniaapiextensionshosting)
- [ManiaAPI.ManiaPlanetAPI](#maniaapimaniaplanetapi)
- [ManiaAPI.ManiaPlanetAPI.Extensions.Hosting](#maniaapimaniaplanetapiextensionshosting)
- [ManiaAPI.TrackmaniaIO](#maniaapitrackmaniaio)
- [ManiaAPI.TMX](#maniaapitmx)
- [ManiaAPI.TMX.Extensions.Gbx](#maniaapitmxextensionsgbx)
- [ManiaAPI.XmlRpc](#maniaapixmlrpc)

## ManiaAPI.NadeoAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.NadeoAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.NadeoAPI/)

Wraps the official Nadeo API used in the latest Trackmania (2020). **This API requires authorization.**

After initial authentication, the connectivity is managed by the library, so you don't have to worry about refreshing the token.

The game provides 3 domains, and they are split into 3 separate services:

- `NadeoServices` for the core functionality
- `NadeoLiveServices` for leaderboards, clubs, and other live content
- `NadeoMeetServices` for getting the current Cup of the Day

#### Features

For `NadeoServices`:

- Get map records
- Get account records
- Get player zones
- Get API routes
- Get all available zones
- Get player club tags
- Get map info

For `NadeoLiveServices`:

- **Edit club campaigns**
- **Edit club activities**
- Get map info
- Get map leaderboards
- Get map medal records
- Get seasonal campaigns
- Get weekly shorts
- Get TOTDs
- Get club campaigns
- Get club info
- Get club members
- Get club activities
- Get club rooms
- Get player season rankings
- Get active advertisements
- Join daily channel (COTD)

For `NadeoMeetServices`:

- Get the current Cup of the Day

#### Setup for a single service

```cs
using ManiaAPI.NadeoAPI;

var ns = new NadeoServices();

await ns.AuthorizeAsync("mylogin", "mypassword", AuthorizationMethod.UbisoftAccount);

// Ready to use
var zones = await ns.GetZonesAsync();
```

You can also use a dedicated server. Just be aware it has some limitations.

```cs
await ns.AuthorizeAsync("my_dedicated_server", "ls>97jO>e3>>D/Ce", AuthorizationMethod.DedicatedServer);
```

For other services, just replace `NadeoServices` with `NadeoLiveServices` or `NadeoMeetServices`.

#### Setup for multiple services

```cs
using ManiaAPI.NadeoAPI;

var login = "mylogin";
var password = "mypassword";

var ns = new NadeoServices();
await ns.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);

var nls = new NadeoLiveServices();
await nls.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);

// Ready to use combined

// With NadeoLiveServices
var weeklyCampaigns = await nls.GetSeasonalCampaignsAsync(1);
var campaignMap = weeklyCampaigns.CampaignList.First().Playlist.First();
var mapInfo = await nls.GetMapInfoAsync(campaignMap.MapUid);
var mapLeaderboard = await nls.GetTopLeaderboardAsync(campaignMap.MapUid);

// With NadeoServices
var records = await ns.GetMapRecordsAsync(mapLeaderboard.Top.Top.Select(x => x.AccountId), mapInfo.MapId);
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.NadeoAPI;

builder.Services.AddHttpClient<NadeoServices>();
builder.Services.AddHttpClient<NadeoLiveServices>();
builder.Services.AddHttpClient<NadeoMeetServices>();

// Do the setup
var login = "mylogin";
var password = "mypassword";

var ns = provider.GetRequiredService<NadeoServices>();
var nls = provider.GetRequiredService<NadeoLiveServices>();
var nms = provider.GetRequiredService<NadeoMeetServices>();

await ns.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);
await nls.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);
await nms.AuthorizeAsync(login, password, AuthorizationMethod.UbisoftAccount);
```

### ManiaAPI.NadeoAPI.Extensions.Gbx

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.NadeoAPI.Extensions.Gbx?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.NadeoAPI.Extensions.Gbx/)

Connects `ManiaAPI.NadeoAPI` with [GBX.NET](https://github.com/BigBang1112/gbx-net) features to provide convenient map upload and **map update**.

#### Features

- Upload a map
- Update a map

#### Example

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

## ManiaAPI.TrackmaniaAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaAPI/)

Wraps https://api.trackmania.com/doc (Trackmania web API). **This API requires authorization.**

#### Features

- Get display names
- Get account IDs from display names
- Get user's map records

More will be added in the future.

#### Setup

For the list of scopes, see [the API docs](https://api.trackmania.com/doc). Generate your credentials [here](https://api.trackmania.com/manager).

```cs
using ManiaAPI.TrackmaniaAPI;

var tm = new TrackmaniaAPI();

await tm.AuthorizeAsync("clientId", "clientSecret", new[] { "clubs", "read_favorite" });

// Ready to use
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.TrackmaniaAPI;

builder.Services.AddHttpClient<TrackmaniaAPI>();

// Do the setup
var tm = provider.GetRequiredService<TrackmaniaAPI>();
await tm.AuthorizeAsync("clientId", "clientSecret", new[] { "clubs", "read_favorite" });

// Ready to use
```

### ManiaAPI.TrackmaniaAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaAPI.Extensions.Hosting/)

Provides Trackmania OAuth2 authorization for ASP.NET Core applications.

#### Setup

For the list of scopes, see [the API docs](https://api.trackmania.com/doc). Generate your credentials [here](https://api.trackmania.com/manager).

```cs
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddTrackmania(options =>
    {
        options.ClientId = config["OAuth2:Trackmania:Id"];
        options.ClientSecret = config["OAuth2:Trackmania:Secret"];

        options.Scope.Add("clubs");
    });
```

## ManiaAPI.ManiaPlanetAPI

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI/)

Wraps https://maniaplanet.com/swagger (ManiaPlanet web API). This API does not require authorization, but you can authorize to have more methods available.

#### Features

[All available on Swagger.](https://maniaplanet.com/swagger)

#### Setup

For the list of scopes, see [here at the bottom](https://doc.maniaplanet.com/web-services/oauth2). Generate your credentials [here](https://maniaplanet.com/web-services-manager).

```cs
using ManiaAPI.ManiaPlanetAPI;

var mp = new ManiaPlanetAPI();

// You can optionally authorize to do more things, and possibly be less limited
await mp.AuthorizeAsync("clientId", "clientSecret", ["basic", "dedicated", "maps"]);

// Ready to use
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.ManiaPlanetAPI;

builder.Services.AddHttpClient<ManiaPlanetAPI>();
```

### ManiaAPI.ManiaPlanetAPI.Extensions.Hosting

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.ManiaPlanetAPI.Extensions.Hosting/)

Provides ManiaPlanet OAuth2 authorization for ASP.NET Core applications.

#### Setup

For the list of scopes, see [here at the bottom](https://doc.maniaplanet.com/web-services/oauth2). Generate your credentials [here](https://maniaplanet.com/web-services-manager).

```cs
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddManiaPlanet(options =>
    {
        options.ClientId = config["OAuth2:ManiaPlanet:Id"];
        options.ClientSecret = config["OAuth2:ManiaPlanet:Secret"];

        Array.ForEach(new[] { "basic", "dedicated", "titles" }, options.Scope.Add);
    });
```

## ManiaAPI.TrackmaniaIO

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TrackmaniaIO?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TrackmaniaIO/)

Wraps the https://trackmania.io/ API which provides services around the Nadeo API.

This API is more moderated and cached, but doesn't require you to authorize with it.

#### Features

- Get various campaigns (including weekly shorts)
- Get leaderboards
- Get recent world records
- Get map info
- Get clubs, including their members and activities
- Get club rooms
- Get track of the days
- Get advertisements
- Get competitions

#### Setup

```cs
using ManiaAPI.TrackmaniaIO;

var tmio = new TrackmaniaIO("Example from ManiaAPI.NET"); // User agent to comply with https://openplanet.dev/tmio/api
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.TrackmaniaIO;

builder.Services.AddHttpClient<TrackmaniaIO>();
```

## ManiaAPI.TMX

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TMX?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TMX/)

Wraps https://tm-exchange.com/ (old TMX).

#### Features

- Get replays
- Search leaderboards
- Search trackpacks
- Search tracks
- Search users
- Get Gbx URLs and HTTP responses
- Get image URLs and HTTP responses

#### Setup

```cs
using ManiaAPI.TMX;

// Pick one from TMUF, TMNF, Nations, Sunrise, Original
var tmx = new TMX(TmxSite.TMUF);
```

### ManiaAPI.TMX.Extensions.Gbx

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TMX.Extensions.Gbx?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TMX.Extensions.Gbx/)

Connects `ManiaAPI.TMX` with [GBX.NET](https://github.com/BigBang1112/gbx-net) features.

#### Features

- Get track Gbx header
- Get track Gbx object
- Get replay Gbx header
- Get replay Gbx object

#### Example

```cs
using ManiaAPI.TMX;
using ManiaAPI.TMX.Extensions.Gbx;

// Pick one from TMUF, TMNF, Nations, Sunrise, Original
var tmx = new TMX(TmxSite.TMUF);

// Get the track object
var map = await tmx.GetTrackGbxNodeAsync(12345);

Console.WriteLine("Number of blocks: " + map.GetBlocks().Count());
```

## ManiaAPI.XmlRpc

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.XmlRpc?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.XmlRpc/)

Wraps TMF, TMT, and ManiaPlanet XML-RPC ingame APIs. **Does not include dedicated server XML-RPC.**

It currently **does not support any authentication** for its complexity and security reasons. If some of the leaderboard methods will become secured with authentication though, this will be considered. For authenticated functionality in TMUF, use the [TMF.NET](https://github.com/Laiteux/TMF.NET) library.

For dedicated server XML-RPC, use the [GbxRemote.Net](https://github.com/EvoEsports/GbxRemote.Net) library.

#### Features

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

#### Setup for TMUF

```cs
using ManiaAPI.XmlRpc;

var masterServer = new MasterServerTMUF();
```

or with DI, using an injected `HttpClient`:

```cs
using ManiaAPI.XmlRpc;

builder.Services.AddHttpClient<MasterServerTMUF>(client => client.BaseAddress = new Uri(MasterServerTMUF.DefaultAddress));
```

#### Setup for ManiaPlanet

First examples assume `Maniaplanet relay 2` master server is still running.

```cs
using ManiaAPI.XmlRpc;

var masterServer = new MasterServerMP4();
```

Because the responses can be quite large sometimes, it's **recommended to accept compression** on the client.

```cs
using ManiaAPI.XmlRpc;

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
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
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    });
```

In case `Maniaplanet relay 2` shuts down / errors out, you have to reach out to init server with `GetWaitingParams` and retrieve an available relay. That's how the game client does it (thanks Mystixor for figuring this out).

To be most inline with the game client, you should validate the master server first with `ValidateAsync`. Behind the scenes, it first requests `GetApplicationConfig`, then on catched HTTP exception, it requests `GetWaitingParams` from the init server and use the available master server instead.

```cs
using ManiaAPI.XmlRpc;

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
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
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
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

#### Setup for TMT

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

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
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
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
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
    builder.Services.AddHttpClient($"{nameof(InitServerTMT)}_{platform}")
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
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

## PLEASE do not use this library to spam the APIs!

Respect the rate limits and cache the responses where possible. Caching is not done internally to give you better control.

If you need to make a lot of requests, do so in a way that doesn't overwhelm the APIs.