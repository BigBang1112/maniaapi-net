namespace ManiaAPI.TMX.Tests.Live;

[LiveTest]
[NotInParallel("Exchange")]
public class ExchangeLiveTests
{
    /// <summary>
    /// Tracks with replays on each site, picked from the most popular tracks, so they won't be deleted.
    /// </summary>
    [Test]
    [Arguments(TmxSite.TMUF, 754327)]
    [Arguments(TmxSite.TMNF, 2233)]
    public async Task TMX_ReplaysOfTrack_CanBeDownloaded(TmxSite site, long trackId)
    {
        using var tmx = new TMX(site);

        var replays = await tmx.GetReplaysAsync(new TMX.GetReplaysParameters { TrackId = trackId });
        await Assert.That(replays.Results).IsNotEmpty();

        await Assert.That(replays.Results).DoesNotContain(x => x.ReplayId <= 0 || x.ReplayTime.TotalMilliseconds <= 0);

        using var replay = await tmx.GetReplayGbxResponseAsync(replays.Results[0]);
        await Assert.That(replay.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Arguments(TmxSite.TMUF)]
    [Arguments(TmxSite.TMNF)]
    [Arguments(TmxSite.Nations)]
    [Arguments(TmxSite.Sunrise)]
    [Arguments(TmxSite.Original)]
    public async Task TMX_SearchTracks_ReturnsDownloadableTracks(TmxSite site)
    {
        using var tmx = new TMX(site);

        var tracks = await tmx.SearchTracksAsync(new TMX.SearchTracksParameters { Count = 5 });
        await Assert.That(tracks.Results).IsNotEmpty();
        await Assert.That(tracks.Results).DoesNotContain(x => x.TrackId <= 0 || string.IsNullOrEmpty(x.TrackName) || string.IsNullOrEmpty(x.UId));

        using var gbx = await tmx.GetTrackGbxResponseAsync(tracks.Results[0]);
        await Assert.That(gbx.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task TMX_TrackImages_CanBeDownloaded()
    {
        using var tmx = new TMX(TmxSite.TMNF);

        var tracks = await tmx.SearchTracksAsync(new TMX.SearchTracksParameters { Count = 20 });
        var withThumbnail = tracks.Results.First(x => x.HasThumbnail);
        var withImage = tracks.Results.First(x => x.Images is { Count: > 0 });

        using var thumbnail = await tmx.GetTrackThumbnailResponseAsync(withThumbnail);
        using var image = await tmx.GetTrackImageResponseAsync(withImage, 0);

        await AssertImageAsync(thumbnail);
        await AssertImageAsync(image);
    }

    [Test]
    public async Task TMX_OtherSearches_ReturnResults()
    {
        using var tmx = new TMX(TmxSite.TMNF);

        var leaderboards = await tmx.SearchLeaderboardsAsync(new TMX.SearchLeaderboardsParameters { Count = 5 });
        var trackpacks = await tmx.SearchTrackpacksAsync(new TMX.SearchTrackpacksParameters { Count = 5 });
        var users = await tmx.SearchUsersAsync(new TMX.SearchUsersParameters { Count = 5 });

        await Assert.That(leaderboards.Results).IsNotEmpty();
        await Assert.That(trackpacks.Results).IsNotEmpty();
        await Assert.That(trackpacks.Results).DoesNotContain(x => x.PackId <= 0 || string.IsNullOrEmpty(x.PackName));
        await Assert.That(users.Results).IsNotEmpty();
    }

    [Test]
    public async Task TMX_Random_ResolvesTrackAndTrackpack()
    {
        using var tmx = new TMX(TmxSite.TMNF);

        var trackId = await tmx.GetRandomTrackIdAsync(new TMX.SearchTracksParameters());
        var track = await tmx.GetRandomTrackAsync(new TMX.SearchTracksParameters());
        var trackpackId = await tmx.GetRandomTrackpackIdAsync(new TMX.SearchTrackpacksParameters());
        var trackpack = await tmx.GetRandomTrackpackAsync(new TMX.SearchTrackpacksParameters());

        await Assert.That(trackId ?? 0).IsGreaterThan(0);
        await Assert.That(track?.TrackId ?? 0).IsGreaterThan(0);
        await Assert.That(trackpackId ?? 0).IsGreaterThan(0);
        await Assert.That(trackpack?.PackId ?? 0).IsGreaterThan(0);
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Maniaplanet)]
    public async Task MX_SearchMaps_ReturnsDownloadableMaps(MxSite site)
    {
        using var mx = new MX(site);

        var maps = await mx.SearchMapsAsync(new MX.SearchMapsParameters { Count = 5 });
        await Assert.That(maps.Results).IsNotEmpty();
        await Assert.That(maps.Results).DoesNotContain(x => x.MapId <= 0 || string.IsNullOrEmpty(x.Name) || string.IsNullOrEmpty(x.MapUid));

        using var gbx = await mx.GetMapGbxResponseAsync(maps.Results[0]);
        await Assert.That(gbx.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Maniaplanet)]
    public async Task MX_MapImages_CanBeDownloaded(MxSite site)
    {
        using var mx = new MX(site);

        var maps = await mx.SearchMapsAsync(new MX.SearchMapsParameters { Count = 20 });
        var withThumbnail = maps.Results.First(x => x.HasThumbnail);
        var withImage = maps.Results.First(x => x.Images is { Count: > 0 });
        var position = withImage.Images![0].Position;

        using var thumbnail = await mx.GetMapThumbnailResponseAsync(withThumbnail);
        using var image = await mx.GetMapImageResponseAsync(withImage, position);
        // The screen endpoint needs a position: without one, it answers 404.
        using var screen = await mx.GetMapScreenResponseAsync(withImage, position);

        await AssertImageAsync(thumbnail);
        await AssertImageAsync(image);
        await AssertImageAsync(screen);
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Maniaplanet)]
    public async Task MX_Random_ResolvesMapAndMappack(MxSite site)
    {
        using var mx = new MX(site);

        var mapId = await mx.GetRandomMapIdAsync(new MX.SearchMapsParameters());
        var map = await mx.GetRandomMapAsync(new MX.SearchMapsParameters());
        var mappackId = await mx.GetRandomMappackIdAsync(new MX.SearchMappacksParameters());
        var mappack = await mx.GetRandomMappackAsync(new MX.SearchMappacksParameters());

        await Assert.That(mapId ?? 0).IsGreaterThan(0);
        await Assert.That(map?.MapId ?? 0).IsGreaterThan(0);
        await Assert.That(mappackId ?? 0).IsGreaterThan(0);
        await Assert.That(mappack?.MappackId ?? 0).IsGreaterThan(0);

        using var mappackThumbnail = await mx.GetMappackThumbnailResponseAsync(mappack!);
        // Not every mappack has a thumbnail, so this only checks that the request goes through.
        await Assert.That(mappackThumbnail.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound).IsTrue();
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Maniaplanet)]
    [Arguments(MxSite.Shootmania)]
    public async Task MX_Metadata_IsAvailable(MxSite site)
    {
        using var mx = new MX(site);

        var tags = await mx.GetTagsAsync();
        var mapOrders = await mx.GetMapSearchOrdersAsync();
        var mappackOrders = await mx.GetMappackSearchOrdersAsync();
        var userOrders = await mx.GetUserSearchOrdersAsync();
        var maptypes = await mx.GetMaptypesAsync();
        var vehicles = await mx.GetVehiclesAsync();
        var titlepacks = await mx.GetTitlepacksAsync();

        await Assert.That(tags).IsNotEmpty();
        await Assert.That(mapOrders).IsNotEmpty();
        await Assert.That(mapOrders).DoesNotContain(x => string.IsNullOrEmpty(x.Name));
        await Assert.That(mappackOrders).IsNotEmpty();
        await Assert.That(userOrders).IsNotEmpty();
        await Assert.That(maptypes).IsNotEmpty();
        await Assert.That(vehicles).IsNotEmpty();
        await Assert.That(titlepacks).IsNotNull();
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Maniaplanet)]
    [Arguments(MxSite.Shootmania)]
    public async Task MX_OtherSearches_ReturnResults(MxSite site)
    {
        using var mx = new MX(site);

        var users = await mx.SearchUsersAsync(new MX.SearchUsersParameters { Count = 5 });
        var mappacks = await mx.SearchMappacksAsync(new MX.SearchMappacksParameters { Count = 5 });

        await Assert.That(users.Results).IsNotEmpty();
        await Assert.That(users.Results).DoesNotContain(x => x.UserId <= 0 || string.IsNullOrEmpty(x.Name));
        await Assert.That(mappacks.Results).IsNotEmpty();
        await Assert.That(mappacks.Results).DoesNotContain(x => x.MappackId <= 0 || string.IsNullOrEmpty(x.Name));
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Maniaplanet)]
    public async Task MX_SearchVideos_ReturnsVideos(MxSite site)
    {
        using var mx = new MX(site);

        var videos = await mx.SearchVideosAsync(new MX.SearchVideosParameters());

        await Assert.That(videos.Results).IsNotEmpty();
        await Assert.That(videos.Results).DoesNotContain(x => x.VideoId <= 0 || string.IsNullOrEmpty(x.Title));
    }

    /// <summary>
    /// Popular maps with many replays on each site.
    /// </summary>
    [Test]
    [Arguments(MxSite.Trackmania, 120839)]
    [Arguments(MxSite.Maniaplanet, 224915)]
    public async Task MX_SearchReplays_ReturnsReplaysOfMap(MxSite site, long mapId)
    {
        using var mx = new MX(site);

        var replays = await mx.SearchReplaysAsync(new MX.SearchReplaysParameters { MapId = mapId });

        await Assert.That(replays.Results).IsNotEmpty();
        await Assert.That(replays.Results).DoesNotContain(x => x.ReplayId <= 0 || x.ReplayTime.TotalMilliseconds <= 0);
    }

    private static async Task AssertImageAsync(HttpResponseMessage response)
    {
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(response.Content.Headers.ContentType?.MediaType ?? "").StartsWith("image/");
    }
}
