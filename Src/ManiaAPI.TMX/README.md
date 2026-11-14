# ManiaAPI.TMX

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TMX?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TMX/)

Wraps:
- https://tm.mania.exchange
- https://trackmania.exchange	
- https://sm.mania.exchange	
- https://tmuf.exchange
- https://tmnf.exchange	
- https://original.tm-exchange.com	
- https://sunrise.tm-exchange.com	
- https://nations.tm-exchange.com
- https://item.exchange (as `IX`)

### Setup


> [!WARNING] \
> Note that `TMX` and `MX` are classes for covering different sets of games. Examples below will mostly use `TMX`. But you can use them with `MX` too.

```cs
using ManiaAPI.TMX;

// Pick one from TMUF, TMNF, Nations, Sunrise, Original
var tmx = new TMX(TmxSite.TMUF);

// or one from Trackmania, Maniaplanet, Shootmania
var tmx = new MX(TMXSite.Trackmania);
```

or with DI, for a specific site, using an injected `HttpClient`:

```cs
using ManiaAPI.TMX;

builder.Services.AddHttpClient("TMX_TMNF");
builder.Services.AddScoped<TMX>(provider => new TMX(
    provider.GetRequiredService<IHttpClientFactory>().CreateClient("TMX_TMNF"), TmxSite.TMNF));
```

For multiple sites in DI, you can use keyed services:

```cs
foreach (var site in Enum.GetValues<TmxSite>())
{
    if (site.isMX()) {
        services.AddHttpClient($"{nameof(MX)}_{site}");
        services.AddKeyedScoped(site, (provider, key) => new MX(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(MX)}_{key}"), site, options));
        services.AddScoped(provider => provider.GetRequiredKeyedService<MX>(site));
    }
    else {
        services.AddHttpClient($"{nameof(TMX)}_{site}");
        services.AddKeyedScoped(site, (provider, key) => new TMX(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(TMX)}_{key}"), site, options));
        services.AddScoped(provider => provider.GetRequiredKeyedService<TMX>(site));
    }
}

services.AddScoped(provider => Enum.GetValues<TmxSite>().Where(site => site.isMX())
    .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<MX>(site)));
services.AddScoped(provider => Enum.GetValues<TmxSite>().Where(site => !site.isMX())
    .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<TMX>(site)));
```

Features this last setup brings:

- You can inject `ImmutableDictionary<TmxSite, TMX>` to get all TMX sites as individual instances
- If you don't need specific site context, you can inject `IEnumerable<TMX>` to get all TMX sites
- Specific `TMX` can be injected using `[FromKeyedServices(TmxSite.TMNF)]`

> [!WARNING]
> If you just inject `TMX` alone, it will give the last-registered one (in this case, Original). If you need a specific site, use `[FromKeyedServices(...)]`.

### Get Replays

```cs
var replayCollection = await tmx.GetReplaysAsync(new()
{
    TrackId = 4808334,
    Count = 20
});

foreach (var item in replayCollection.Results)
{
    Console.WriteLine(item.ReplayTime);
})
```

### Search Tracks

```cs
var trackCollection = await tmx.SearchTracksAsync(new()
{
    Name = "wirtual", // tracks that have wirtual in their name
    Count = 20
});

foreach (var item in trackCollection.Results)
{
    Console.WriteLine(item.TrackName);
})
```

### Search Leaderboards

```cs
var leaderboardCollection = await tmx.SearchLeaderboardsAsync(new()
{
    Count = 10
});

foreach (var item in leaderboardCollection.Results)
{
    Console.WriteLine(item.User.Name);
}
```

### Search Trackpacks

```cs
var trackpackCollection = await tmx.SearchTrackpacksAsync(new()
{
    Count = 15
});

foreach (var item in trackpackCollection.Results)
{
    Console.WriteLine(item.PackName);
}
```

### Search Users

```cs
var userCollection = await tmx.SearchUsersAsync(new()
{
    InModerators = true,
    Count = 10
});

foreach (var item in userCollection.Results)
{
    Console.WriteLine(item.Name);
}
```

### Search Videos

```cs
var videoCollection = await tmx.SearchVideosAsync(new()
{
    TrackId = [4808334],
    Count = 10
});

foreach (var item in videoCollection.Results)
{
    Console.WriteLine($"{item.Title}: {item.Link}");
}
```

### Get site metadata

Search orders, tags, track types and vehicles of the site, for the values used in search parameters:

```cs
var trackOrders = await tmx.GetTrackSearchOrdersAsync(); // also trackpack, user, video and leaderboard orders
var tags = await tmx.GetTagsAsync();
var tagDescriptions = await tmx.GetTagDescriptionsAsync();
var trackTypes = await tmx.GetTrackTypesAsync();
var vehicles = await tmx.GetVehiclesAsync();
```

### Item Exchange

`IX` wraps https://item.exchange, which hosts custom items, blocks and item sets for ManiaPlanet and Trackmania. It has no v2 API, so it uses the v1 methods the MX API documentation lists as active.

```cs
using ManiaAPI.TMX;

var ix = new IX();

// Search items, 10 to 200 per page
var result = await ix.SearchItemsAsync(new()
{
    ItemName = ["rock"],
    Collections = [IXEnvironment.Valley],
    Limit = 20
});

foreach (var item in result.Results)
{
    Console.WriteLine($"{item.Name} ({item.TypeName}) by {item.Username}");
}

// Items or sets by ID, a user's basket, and random picks
var items = await ix.GetItemsAsync([256, 40503]);
var sets = await ix.GetItemSetsAsync([41]); // with their items
var basket = await ix.GetBasketAsync(1814); // null for an unknown user
var random = await ix.GetRandomItemAsync(new() { Game = IXGame.Trackmania });

// Downloads and images
using var download = await ix.GetItemDownloadResponseAsync(40503); // .Gbx, or .zip when an item needs several files
string icon = ix.GetItemIconUrl(256);
string setImage = ix.GetItemSetImageUrl(41, position: 1);
```

### Get Replay Gbx

Just the URL:

```cs
string url = tmx.GetReplayGbxUrl(replayId: 5032240);
```

Or request it:

```cs
using HttpResponseMessage response = await tmx.GetReplayGbxResponseAsync(replayId: 5032240);
```

Or use `ManiaAPI.TMX.Extensions.Gbx` package to load it into `CGameCtnReplayRecord`, either just the header or full Gbx:

```cs
using ManiaAPI.TMX.Extensions.Gbx;

// Just the header
Gbx<CGameCtnReplayRecord> gbxReplayHeader = await tmx.GetReplayGbxHeaderAsync(replayId: 5032240);

// Full Gbx
Gbx<CGameCtnReplayRecord> gbxReplay = await tmx.GetReplayGbxAsync(replayId: 5032240);
```

### Get Track Gbx

Just the URL:

```cs
string url = tmx.GetTrackGbxUrl(trackId: 4808334);
```

Or request it:

```cs
using HttpResponseMessage response = await tmx.GetTrackGbxResponseAsync(trackId: 4808334);
```

Or use `ManiaAPI.TMX.Extensions.Gbx` package to load it into `CGameCtnChallenge`, either just the header or full Gbx:

```cs
using ManiaAPI.TMX.Extensions.Gbx;

// Just the header
Gbx<CGameCtnChallenge> gbxMapHeader = await tmx.GetTrackGbxHeaderAsync(trackId: 4808334);

// Full Gbx
Gbx<CGameCtnChallenge> gbxMap = await tmx.GetTrackGbxAsync(trackId: 4808334);
```

### Get Track Thumbnail

Just the URL:

```cs
string url = tmx.GetTrackThumbnailUrl(trackId: 4808334);
```

Or request it:

```cs
using HttpResponseMessage response = await tmx.GetTrackThumbnailResponseAsync(trackId: 4808334);
```

### Get Track Image

Just the URL:

```cs
string url = tmx.GetTrackImageUrl(trackId: 4808334, imageIndex: 0);
```

Or request it:

```cs
using HttpResponseMessage response = await tmx.GetTrackImageResponseAsync(trackId: 4808334, imageIndex: 0);
```