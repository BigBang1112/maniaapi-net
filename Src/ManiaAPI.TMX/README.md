# ManiaAPI.TMX

[![NuGet](https://img.shields.io/nuget/vpre/ManiaAPI.TMX?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/ManiaAPI.TMX/)

Wraps https://tm-exchange.com/ (old TMX).

### Setup

```cs
using ManiaAPI.TMX;

// Pick one from TMUF, TMNF, Nations, Sunrise, Original
var tmx = new TMX(TmxSite.TMUF);
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
foreach (TmxSite site in Enum.GetValues<TmxSite>())
{
    builder.Services.AddHttpClient($"{nameof(TMX)}_{site}");

    builder.Services.AddKeyedScoped(site, (provider, key) => new TMX(
        provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(TMX)}_{key}"), site));
    builder.Services.AddScoped(provider => provider.GetRequiredKeyedService<TMX>(site));
}

builder.Services.AddScoped(provider => Enum.GetValues<TmxSite>()
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