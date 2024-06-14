# ManiaAPI.NET

Currently a wrapper for these web APIs:

- Nadeo API (official TM2020 ingame API)
- Trackmania web API
- Trackmania.io
- Trackmania Exchange

Soon:

- ManiaPlanet web API
- XML-RPC (for TMF and ManiaPlanet)

This set of libraries was made to be very easy and straight-forward to use, but also easily mocked, so that it can be integrated into the real world in no time.

## ManiaAPI.TMX

Wraps https://tm-exchange.com/ (old TMX).

### Setup

```cs
using ManiaAPI.TMX;

// Pick one from TMUF, TMNF, Nations, Sunrise, Original
var tmx = new TMX(TmxSite.TMUF);
```

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

Use `ManiaAPI.TMX.Extensions.Gbx` package to load a replay into `CGameCtnReplayRecord`, either just the header or full Gbx:

```cs
using ManiaAPI.TMX.Extensions.Gbx;

// Just the header
Gbx<CGameCtnReplayRecord> gbxMapHeader = await tmx.GetReplayGbxHeaderAsync(replayId: 5032240);

// Full Gbx
Gbx<CGameCtnReplayRecord> gbxMap = await tmx.GetReplayGbxAsync(replayId: 5032240);
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