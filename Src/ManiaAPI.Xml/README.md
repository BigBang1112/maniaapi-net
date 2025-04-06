# ManiaAPI.Xml

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.Xml?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.Xml/)

Wraps TMF, TMT, and ManiaPlanet XML ingame APIs. **Does not relate to the dedicated server XML-RPC.**

It currently **does not support any authentication** for its complexity and security reasons. If some of the leaderboard methods will become secured with authentication though, this will be considered. For authenticated functionality in TMUF, use the [TMF.NET](https://github.com/Laiteux/TMF.NET) library.

For dedicated server XML, use the [GbxRemote.Net](https://github.com/EvoEsports/GbxRemote.Net) library.

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
using ManiaAPI.Xml;

var masterServer = new MasterServerTMUF();
```

## Setup for ManiaPlanet

First examples assume `Maniaplanet relay 2` master server is still running.

```cs
using ManiaAPI.Xml;

var masterServer = new MasterServerMP4();
```

Because the responses can be quite large sometimes, it's **recommended to accept compression** on the client.

```cs
using ManiaAPI.Xml;

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
{
    BaseAddress = new Uri(MasterServerMP4.DefaultAddress)
};
var masterServer = new MasterServerMP4(httpClient);
```

In case `Maniaplanet relay 2` shuts down / errors out, you have to reach out to init server with `GetWaitingParams` and retrieve an available relay. That's how the game client does it (thanks Mystixor for figuring this out).

To be most inline with the game client, you should validate the master server first with `ValidateAsync`. Behind the scenes, it first requests `GetApplicationConfig`, then on catched HTTP exception, it requests `GetWaitingParams` from the init server and use the available master server instead.

```cs
using ManiaAPI.Xml;

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
{
    BaseAddress = new Uri(MasterServerMP4.DefaultAddress)
};
var masterServer = new MasterServerMP4(httpClient);

await masterServer.ValidateAsync(); // Do this for reliability

// The master server is now ready to use
```

## Setup for TMT

TMT handles 3 platforms: PC, XB1, and PS4. Each have their own init server and master server. Nadeo still tends to change these master servers, so it's recommended to first go through the init server.

```cs
using ManiaAPI.Xml;

var initServer = new InitServerTMT(Platform.PC);
var waitingParams = await initServer.GetWaitingParamsAsync();

var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

// You can repeat this exact setup for XB1 and PS4 as well if you want to work with those platforms, with something like Dictionary<Platform, MasterServerTMT> ...
```

Because the responses can be quite large sometimes, it's **recommended to accept compression** on the client for the **master server**. Init server does not return large responses, so it's not necessary for that one.

```cs
using ManiaAPI.Xml;

var initServer = new InitServerTMT(Platform.PC);
var waitingParams = await initServer.GetWaitingParamsAsync();

var httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
{
    BaseAddress = waitingParams.MasterServers.First().GetUri()
};
var masterServer = new MasterServerTMT(httpClient);

// You can repeat this exact setup for XB1 and PS4 as well if you want to work with those platforms, with something like Dictionary<Platform, MasterServerTMT> ...
```

**For a simple setup with multiple platforms, the `AggregatedMasterServerTMT` is recommended:**

```cs
using ManiaAPI.Xml;

var waitingParams = Enum.GetValues<Platform>().ToDictionary(
    platform => platform, 
    platform => new InitServerTMT(platform).GetWaitingParamsAsync(cancellationToken));

await Task.WhenAll(waitingParams.Values);

var aggregatedMasterServer = new AggregatedMasterServerTMT(waitingParams.ToDictionary(
    pair => pair.Key,
    pair => new MasterServerTMT(
        new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
        {
            BaseAddress = pair.Value.Result.MasterServers.First().GetUri()
        })
    ));

// You can now use aggregatedMasterServer to work with all master servers at once
```