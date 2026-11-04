namespace ManiaAPI.TMX.Tests.Live;

[Category("Live"), LiveTest, Retry(2)]
[NotInParallel("Exchange")]
public class ExchangeLiveTests
{
    [Test]
    [Arguments(TmxSite.TMUF, 754327)]
    [Arguments(TmxSite.TMNF, 2233)]
    public async Task TMX_ReplaysOfTrack_CanBeDownloaded(TmxSite site, long trackId)
    {
        using var tmx = new TMX(site);

        var replays = await tmx.GetReplaysAsync(new TMX.GetReplaysParameters { TrackId = trackId });
        await Assert.That(replays.Results).IsNotEmpty();

        using var replay = await tmx.GetReplayGbxResponseAsync(replays.Results[0]);
        await Assert.That(replay.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task TMX_SearchTracks_ReturnsDownloadableTracks()
    {
        using var tmx = new TMX(TmxSite.TMNF);

        var tracks = await tmx.SearchTracksAsync(new TMX.SearchTracksParameters { Count = 5 });
        await Assert.That(tracks.Results).IsNotEmpty();

        using var gbx = await tmx.GetTrackGbxResponseAsync(tracks.Results[0]);
        await Assert.That(gbx.IsSuccessStatusCode).IsTrue();
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
        await Assert.That(users.Results).IsNotEmpty();
    }

    [Test]
    public async Task TMX_RandomTrack_ResolvesTrack()
    {
        using var tmx = new TMX(TmxSite.TMNF);

        var track = await tmx.GetRandomTrackAsync(new TMX.SearchTracksParameters());

        await Assert.That(track).IsNotNull();
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Maniaplanet)]
    public async Task MX_SearchMaps_ReturnsDownloadableMaps(MxSite site)
    {
        using var mx = new MX(site);

        var maps = await mx.SearchMapsAsync(new MX.SearchMapsParameters { Count = 5 });
        await Assert.That(maps.Results).IsNotEmpty();

        using var gbx = await mx.GetMapGbxResponseAsync(maps.Results[0]);
        await Assert.That(gbx.IsSuccessStatusCode).IsTrue();
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
        var maptypes = await mx.GetMaptypesAsync();

        await Assert.That(tags).IsNotEmpty();
        await Assert.That(mapOrders).IsNotEmpty();
        await Assert.That(maptypes).IsNotEmpty();
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
        await Assert.That(mappacks.Results).IsNotEmpty();
    }

    [Test]
    [Arguments(MxSite.Trackmania, 120839)]
    [Arguments(MxSite.Maniaplanet, 224915)]
    public async Task MX_SearchReplays_ReturnsReplaysOfMap(MxSite site, long mapId)
    {
        using var mx = new MX(site);

        var replays = await mx.SearchReplaysAsync(new MX.SearchReplaysParameters { MapId = mapId });

        await Assert.That(replays.Results).IsNotEmpty();
    }
}
