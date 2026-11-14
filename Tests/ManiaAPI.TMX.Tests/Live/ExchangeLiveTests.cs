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
    [Arguments(TmxSite.TMUF)]
    [Arguments(TmxSite.TMNF)]
    [Arguments(TmxSite.Nations)]
    [Arguments(TmxSite.Sunrise)]
    [Arguments(TmxSite.Original)]
    public async Task TMX_Metadata_IsAvailable(TmxSite site)
    {
        using var tmx = new TMX(site);

        var trackOrders = await tmx.GetTrackSearchOrdersAsync();
        var trackpackOrders = await tmx.GetTrackpackSearchOrdersAsync();
        var userOrders = await tmx.GetUserSearchOrdersAsync();
        var videoOrders = await tmx.GetVideoSearchOrdersAsync();
        var leaderboardOrders = await tmx.GetLeaderboardSearchOrdersAsync();
        var tags = await tmx.GetTagsAsync();
        var tagDescriptions = await tmx.GetTagDescriptionsAsync();
        var trackTypes = await tmx.GetTrackTypesAsync();
        var vehicles = await tmx.GetVehiclesAsync();

        // Key 0 has no name on these sites, so only the named orders are checked.
        await Assert.That(trackOrders).Contains(x => !string.IsNullOrEmpty(x.Name));
        await Assert.That(trackpackOrders).Contains(x => !string.IsNullOrEmpty(x.Name));
        await Assert.That(userOrders).Contains(x => !string.IsNullOrEmpty(x.Name));
        await Assert.That(videoOrders).Contains(x => !string.IsNullOrEmpty(x.Name));
        await Assert.That(leaderboardOrders).IsNotEmpty();
        await Assert.That(tags).IsNotEmpty();
        await Assert.That(tags).DoesNotContain(x => string.IsNullOrEmpty(x.Name));
        await Assert.That(tagDescriptions).IsNotEmpty();
        await Assert.That(tagDescriptions).DoesNotContain(x => string.IsNullOrEmpty(x.Description));
        await Assert.That(trackTypes).IsNotEmpty();
        await Assert.That(vehicles).IsNotEmpty();
    }

    [Test]
    [Arguments(TmxSite.TMUF)]
    [Arguments(TmxSite.TMNF)]
    public async Task TMX_SearchVideos_ReturnsVideos(TmxSite site)
    {
        using var tmx = new TMX(site);

        var videos = await tmx.SearchVideosAsync(new TMX.SearchVideosParameters { Count = 5 });

        await Assert.That(videos.Results).IsNotEmpty();
        await Assert.That(videos.Results).DoesNotContain(x => x.VideoId <= 0 || string.IsNullOrEmpty(x.Title) || string.IsNullOrEmpty(x.LinkId));

        // Filtering by the first video's ID finds only that video, so the generated query parameters reach the API.
        var first = videos.Results[0];
        var filtered = await tmx.SearchVideosAsync(new TMX.SearchVideosParameters { Id = [first.VideoId] });

        await Assert.That(filtered.Results.Select(x => x.VideoId)).IsEquivalentTo([first.VideoId]);
    }

    /// <summary>
    /// V_Rock4 in the Valley Rocks set and a TM2020 block, both by long-standing uploaders.
    /// </summary>
    [Test]
    public async Task IX_ItemsSetsAndBasket_AreAvailable()
    {
        using var ix = new IX();

        var items = await ix.GetItemsAsync([256, 40503]);
        var sets = await ix.GetItemSetsAsync([41]);
        var basket = await ix.GetBasketAsync(1814);
        var missing = await ix.GetBasketAsync(999999999);

        await Assert.That(items.Select(x => x.Id)).IsEquivalentTo([256L, 40503L]);
        await Assert.That(items.First(x => x.Id == 256).SetId).IsEqualTo(41);
        await Assert.That(items.First(x => x.Id == 40503).Type).IsEqualTo(IXItemType.Block);
        await Assert.That(sets.Single().Items!.Select(x => x.Id)).Contains(256L);
        await Assert.That(basket!.Items).IsNotEmpty();
        await Assert.That(missing).IsNull();
    }

    [Test]
    public async Task IX_Searches_ReturnResults()
    {
        using var ix = new IX();

        var items = await ix.SearchItemsAsync(new IX.SearchItemsParameters { ItemName = ["rock"], Collections = [IXEnvironment.Valley], Limit = 10 });
        var tagged = await ix.SearchItemsAsync(new IX.SearchItemsParameters { Tags = [45, 30], AllTags = true, Limit = 10 });
        var sets = await ix.SearchItemSetsAsync(new IX.SearchItemSetsParameters { Limit = 10 });
        var randomItem = await ix.GetRandomItemAsync(new IX.SearchItemsParameters());
        var randomSet = await ix.GetRandomItemSetAsync(new IX.SearchItemSetsParameters());

        await Assert.That(items.Results).IsNotEmpty();
        await Assert.That(items.Results).DoesNotContain(x => x.Collection != IXEnvironment.Valley || !x.Name.Contains("rock", StringComparison.OrdinalIgnoreCase));
        await Assert.That(items.TotalItemCount).IsGreaterThanOrEqualTo(items.Results.Count);
        await Assert.That(tagged.Results).DoesNotContain(x => !x.Tags!.Split(',').Contains("45") || !x.Tags.Split(',').Contains("30"));
        await Assert.That(sets.Results).Count().IsEqualTo(10);
        await Assert.That(randomItem?.Id ?? 0).IsGreaterThan(0);
        await Assert.That(randomSet?.Id ?? 0).IsGreaterThan(0);
    }

    [Test]
    public async Task IX_DownloadsAndImages_AreServed()
    {
        using var ix = new IX();

        using var gbx = await ix.GetItemDownloadResponseAsync(40503);
        using var zip = await ix.GetItemSetDownloadResponseAsync(41);
        using var icon = await ix.GetItemIconResponseAsync(256);
        using var setImage = await ix.GetItemSetImageResponseAsync(41, 1);

        await Assert.That(gbx.Content.Headers.ContentType?.MediaType).IsEqualTo("application/x-gbx");
        await Assert.That(zip.Content.Headers.ContentType?.MediaType).IsEqualTo("application/zip");
        await AssertImageAsync(icon);
        await AssertImageAsync(setImage);
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

    /// <summary>
    /// Maps with a thumbnail and images on each site, picked from the most awarded maps, so they won't be deleted.
    /// </summary>
    [Test]
    [Arguments(MxSite.Trackmania, 170211)] // Deep Dip 2
    [Arguments(MxSite.Maniaplanet, 22)] // Tiger Blood
    public async Task MX_MapImages_CanBeDownloaded(MxSite site, long mapId)
    {
        using var mx = new MX(site);

        var maps = await mx.SearchMapsAsync(new MX.SearchMapsParameters { Id = [mapId] });
        await Assert.That(maps.Results).HasSingleItem();

        var map = maps.Results[0];
        await Assert.That(map.HasThumbnail).IsTrue();
        await Assert.That(map.Images).IsNotNull().And.IsNotEmpty();

        var position = map.Images![0].Position;

        using var thumbnail = await mx.GetMapThumbnailResponseAsync(map);
        using var image = await mx.GetMapImageResponseAsync(map, position);
        // The screen endpoint needs a position: without one, it answers 404.
        using var screen = await mx.GetMapScreenResponseAsync(map, position);

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
