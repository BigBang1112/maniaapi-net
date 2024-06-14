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

### Methods

```cs
Task<ItemCollection<ReplayItem>> GetReplaysAsync(TMX.GetReplaysParameters parameters, CancellationToken cancellationToken = default);
Task<ItemCollection<LeaderboardItem>> SearchLeaderboardsAsync(TMX.SearchLeaderboardsParameters parameters, CancellationToken cancellationToken = default);
Task<ItemCollection<TrackpackItem>> SearchTrackpacksAsync(TMX.SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default);
Task<ItemCollection<TrackItem>> SearchTracksAsync(TMX.SearchTracksParameters parameters, CancellationToken cancellationToken = default);
Task<ItemCollection<UserItem>> SearchUsersAsync(TMX.SearchUsersParameters parameters, CancellationToken cancellationToken = default);
string GetReplayGbxUrl(long replayId);
string GetReplayGbxUrl(ReplayItem replay);
Task<HttpResponseMessage> GetReplayGbxResponseAsync(long replayId, CancellationToken cancellationToken = default);
Task<HttpResponseMessage> GetReplayGbxResponseAsync(ReplayItem replay, CancellationToken cancellationToken = default);
string GetTrackGbxUrl(long trackId);
string GetTrackGbxUrl(TrackItem track);
Task<HttpResponseMessage> GetTrackGbxResponseAsync(long trackId, CancellationToken cancellationToken = default);
Task<HttpResponseMessage> GetTrackGbxResponseAsync(TrackItem track, CancellationToken cancellationToken = default);
string GetTrackThumbnailUrl(long trackId);
string GetTrackThumbnailUrl(TrackItem track);
Task<HttpResponseMessage> GetTrackThumbnailResponseAsync(long trackId, CancellationToken cancellationToken = default);
Task<HttpResponseMessage> GetTrackThumbnailResponseAsync(TrackItem track, CancellationToken cancellationToken = default);
string GetTrackImageUrl(long trackId, int imageIndex);
string GetTrackImageUrl(TrackItem track, int imageIndex);
Task<HttpResponseMessage> GetTrackImageResponseAsync(long trackId, int imageIndex, CancellationToken cancellationToken = default);
Task<HttpResponseMessage> GetTrackImageResponseAsync(TrackItem track, int imageIndex, CancellationToken cancellationToken = default);
```