using ManiaAPI.TMX.Attributes;
using System.Net.Http.Headers;

namespace ManiaAPI.TMX;

public interface IMX : IClient
{
    MxSite Site { get; }
    string SiteName { get; }

    string GetMapGbxUrl(long mapId);
    string GetMapGbxUrl(MapItem map);
    Task<HttpResponseMessage> GetMapGbxResponseAsync(long mapId, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetMapGbxResponseAsync(MapItem map, CancellationToken cancellationToken = default);
    
    string GetReplayGbxUrl(long replayId);
    string GetReplayGbxUrl(ReplayItemMX replay);
    Task<HttpResponseMessage> GetReplayGbxResponseAsync(long replayId, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetReplayGbxResponseAsync(ReplayItemMX replay, CancellationToken cancellationToken = default);

    string GetMapImageUrl(long mapId, int? position = null);
    string GetMapImageUrl(MapItem map, int? position = null);
    Task<HttpResponseMessage> GetMapImageResponseAsync(long mapId, int? imageIndex = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetMapImageResponseAsync(MapItem map, int? imageIndex = null, CancellationToken cancellationToken = default);

    string GetMapThumbnailUrl(long mapId);
    string GetMapThumbnailUrl(MapItem map);
    Task<HttpResponseMessage> GetMapThumbnailResponseAsync(long mapId, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetMapThumbnailResponseAsync(MapItem map, CancellationToken cancellationToken = default);

    string GetMapScreenUrl(long mapId, int? position = null);
    string GetMapScreenUrl(MapItem map, int? position = null);
    Task<HttpResponseMessage> GetMapScreenResponseAsync(long mapId, int? imageIndex = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetMapScreenResponseAsync(MapItem map, int? imageIndex = null, CancellationToken cancellationToken = default);

    string GetMappackThumbnailUrl(long mappackId);
    string GetMappackThumbnailUrl(MappackItem mappack);
    Task<HttpResponseMessage> GetMappackThumbnailResponseAsync(long mappackId, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetMappackThumbnailResponseAsync(MappackItem mappack, CancellationToken cancellationToken = default);

    Task<ItemCollection<MapItem>> SearchMapsAsync(MX.SearchMapsParameters parameters, CancellationToken cancellationToken = default);

    Task<ItemCollection<ReplayItemMX>> SearchReplaysAsync(MX.SearchReplaysParameters parameters, CancellationToken cancellationToken = default);

    Task<ItemCollection<UserItemMX>> SearchUsersAsync(MX.SearchUsersParameters parameters, CancellationToken cancellationToken = default);
    
    Task<ItemCollection<VideoItem>> SearchVideosAsync(MX.SearchVideosParameters parameters, CancellationToken cancellationToken = default);
    
    Task<ItemCollection<MappackItem>> SearchMappacksAsync(MX.SearchMappacksParameters parameters, CancellationToken cancellationToken = default);
    
    Task<SearchOrderItem[]> GetMapSearchOrdersAsync(CancellationToken cancellationToken = default);

    Task<SearchOrderItem[]> GetUserSearchOrdersAsync(CancellationToken cancellationToken = default);

    Task<SearchOrderItem[]> GetMappackSearchOrdersAsync(CancellationToken cancellationToken = default);

    Task<TagInfo[]> GetTagsAsync(CancellationToken cancellationToken = default);

    Task<string[]> GetMaptypesAsync(CancellationToken cancellationToken = default);

    Task<string[]> GetVehiclesAsync(CancellationToken cancellationToken = default);

    Task<string[]> GetTitlepacksAsync(CancellationToken cancellationToken = default);

}

public partial class MX : IMX
{
    public HttpClient Client { get; }
    public MxSite Site { get; }
    public string SiteName { get; }

    public MX(HttpClient client, MxSite site)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = "2.7.0";

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(TMX; Discord=bigbang1112)"));
        }

        Site = site;
        SiteName = site.ToString();

        var url = site switch {
            MxSite.Maniaplanet => "https://tm.mania.exchange/",
            MxSite.Trackmania => "https://trackmania.exchange/",
            MxSite.Shootmania => "https://sm.mania.exchange/",
            _ => throw new NotImplementedException()
        };

        Client.BaseAddress = new Uri(url);
    }

    public MX(MxSite site) : this(new HttpClient(), site) { }


    public string GetMapGbxUrl(long mapId) => $"{Client.BaseAddress}mapgbx/{mapId}";
    public string GetMapGbxUrl(MapItem map) => GetMapGbxUrl(map.MapId);

    public virtual async Task<HttpResponseMessage> GetMapGbxResponseAsync(long mapId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetMapGbxUrl(mapId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetMapGbxResponseAsync(MapItem map, CancellationToken cancellationToken = default)
    {
        return await GetMapGbxResponseAsync(map.MapId, cancellationToken);
    }
    
    public string GetReplayGbxUrl(long replayId) => $"{Client.BaseAddress}recordgbx/{replayId}";
    public string GetReplayGbxUrl(ReplayItemMX replay) => GetReplayGbxUrl(replay.ReplayId);

    public virtual async Task<HttpResponseMessage> GetReplayGbxResponseAsync(long replayId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetReplayGbxUrl(replayId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetReplayGbxResponseAsync(ReplayItemMX replay, CancellationToken cancellationToken = default)
    {
        return await GetReplayGbxResponseAsync(replay.ReplayId, cancellationToken);
    }

    // TODO: add hq, guid, mappackid, mappacksecret query parameters
    public string GetMapImageUrl(long mapId, int? position = null) => position.HasValue
        ? $"{Client.BaseAddress}mapimage/{mapId}/{position.Value}"
        : $"{Client.BaseAddress}mapimage/{mapId}";
    public string GetMapImageUrl(MapItem map, int? position = null) => GetMapImageUrl(map.MapId, position);

    public virtual async Task<HttpResponseMessage> GetMapImageResponseAsync(long mapId, int? imageIndex = null, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetMapImageUrl(mapId, imageIndex), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetMapImageResponseAsync(MapItem map, int? imageIndex = null, CancellationToken cancellationToken = default)
    {
        return await GetMapImageResponseAsync(map.MapId, imageIndex, cancellationToken);
    }

    // TODO: add thumb, guid, mappackid, mappacksecret query parameters
    public string GetMapScreenUrl(long mapId, int? position = null) => position.HasValue
        ? $"{Client.BaseAddress}mapscreen/{mapId}/{position.Value}"
        : $"{Client.BaseAddress}mapscreen/{mapId}";
    public string GetMapScreenUrl(MapItem map, int? position = null) => GetMapScreenUrl(map.MapId, position);

    public virtual async Task<HttpResponseMessage> GetMapScreenResponseAsync(long mapId, int? imageIndex = null, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetMapScreenUrl(mapId, imageIndex), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetMapScreenResponseAsync(MapItem map, int? imageIndex = null, CancellationToken cancellationToken = default)
    {
        return await GetMapScreenResponseAsync(map.MapId, imageIndex, cancellationToken);
    }

    // TODO: add guid, mappackid, mappacksecret query parameters
    public string GetMapThumbnailUrl(long mapId) => $"{Client.BaseAddress}mapthumb/{mapId}";
    public string GetMapThumbnailUrl(MapItem map) => GetMapThumbnailUrl(map.MapId);

    public virtual async Task<HttpResponseMessage> GetMapThumbnailResponseAsync(long mapId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetMapThumbnailUrl(mapId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetMapThumbnailResponseAsync(MapItem map, CancellationToken cancellationToken = default)
    {
        return await GetMapThumbnailResponseAsync(map.MapId, cancellationToken);
    }

    public string GetMappackThumbnailUrl(long mappackId) => $"{Client.BaseAddress}mappackthumb/{mappackId}";
    public string GetMappackThumbnailUrl(MappackItem mappack) => GetMappackThumbnailUrl(mappack.MappackId);

    public virtual async Task<HttpResponseMessage> GetMappackThumbnailResponseAsync(long mappackId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetMappackThumbnailUrl(mappackId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetMappackThumbnailResponseAsync(MappackItem mappack, CancellationToken cancellationToken = default)
    {
        return await GetMappackThumbnailResponseAsync(mappack.MappackId, cancellationToken);
    }

    [Parameters<MapItem>]
    public readonly partial record struct SearchMapsParameters {
        /// <summary>
        /// Search order, for order modes refer to Get Map Search Orders.
        /// </summary>
        public int? Order1 { get; init; }

        /// <summary>
        /// Secondary search order, for order modes refer to Get Map Search Orders.
        /// </summary>
        public int? Order2 { get; init; }

        /// <summary>
        /// Amount of results (default: 40)
        /// </summary>
        public int? Count { get; init; }

        /// <summary>
        /// Display results after specified MapId and given order - used for paging.
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Display results before specified MapId and given order - used for paging.
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified MapId as well.
        /// </summary>
        public long? From { get; init; }

        /// <summary>
        /// Filter by MapIds, max. 100
        /// </summary>
        public long[]? Id { get; init; }

        /// <summary>
        /// Filter by MapUids, max. 100
        /// </summary>
        public string[]? Uid { get; init; }

        /// <summary>
        /// 1 to query a random map - count of 1 required
        /// </summary>
        public int? Random { get; init; }

        /// <summary>
        /// Filter by Name (Full-text)
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Filter by site username or Name of Authors
        /// </summary>
        public string? Author { get; init; }

        /// <summary>
        /// Filter by site UserId or UserId of Authors
        /// </summary>
        public long? AuthorUserId { get; init; }

        /// <summary>
        /// Filter by VideoId containing the map
        /// </summary>
        public long? VideoId { get; init; }

        /// <summary>
        /// Filter by MappackId containing the map
        /// </summary>
        public long? MappackId { get; init; }

        /// <summary>
        /// Requires mappackid: The Map's status in the Mappack, max. 5
        /// </summary>
        public int[]? Status { get; init; }

        /// <summary>
        /// Filter by UserId having awarded the map
        /// </summary>
        public long? AwardedBy { get; init; }

        /// <summary>
        /// Filter by UserId having commented on the map
        /// </summary>
        public long? CommentedBy { get; init; }

        /// <summary>
        /// Filter by tags, see Get Tags, max. 100
        /// </summary>
        public int[]? Tag { get; init; }

        /// <summary>
        /// Filter by excluding tags, see Get Tags, max. 100
        /// </summary>
        public int[]? ETag { get; init; }

        /// <summary>
        /// true: map must have all specified tags, false: map must have one of the specified tags
        /// </summary>
        [AsNumber]
        public bool? TagInclusive { get; init; }

        /// <summary>
        /// Filter by Type
        /// </summary>
        public int? PrimaryType { get; init; }

        /// <summary>
        /// Filter by MapType, using entries from Get Map Maptypes
        /// </summary>
        public string? MapType { get; init; }

        /// <summary>
        /// Filter by ReplayType
        /// </summary>
        public int? LbType { get; init; }

        /// <summary>
        /// Filter by min. UploadedAt
        /// </summary>
        public DateTimeOffset? UploadedAfter { get; init; }

        /// <summary>
        /// Filter by max. UploadedAt
        /// </summary>
        public DateTimeOffset? UploadedBefore { get; init; }

        /// <summary>
        /// Filter by TitlePack, using entries from Get Map Titlepacks
        /// </summary>
        public string? TitlePack { get; init; }

        /// <summary>
        /// Filter by MapType explicitly
        /// </summary>
        public string? CustomMapType { get; init; }

        /// <summary>
        /// Filter by min. AuthorTime
        /// </summary>
        public int? AuthorTimeMin { get; init; }

        /// <summary>
        /// Filter by max. AuthorTime
        /// </summary>
        public int? AuthorTimeMax { get; init; }

        /// <summary>
        /// Filter by min. Length
        /// </summary>
        public int? LengthMin { get; init; }

        /// <summary>
        /// Filter by max. Length
        /// </summary>
        public int? LengthMax { get; init; }

        /// <summary>
        /// Filter by Environment
        /// </summary>
        public int[]? Environment { get; init; }

        /// <summary>
        /// Filter by excluding Environments
        /// </summary>
        public int[]? ExEnvironment { get; init; }

        /// <summary>
        /// Filter by Vehicle, using entries returned in Get Map Vehicles or using enum Vehicles
        /// </summary>
        public string[]? Vehicle { get; init; }

        /// <summary>
        /// Filter by excluding Vehicle, using entries returned in Get Map Vehicles or using enum Vehicles
        /// </summary>
        public string[]? ExVehicle { get; init; }

        /// <summary>
        /// Filter by Mod (exact match)
        /// </summary>
        public string? Mod { get; init; }

        /// <summary>
        /// Filter by Mood
        /// </summary>
        public int[]? Mood { get; init; }

        /// <summary>
        /// Filter by Difficulty
        /// </summary>
        public int[]? Difficulty { get; init; }

        /// <summary>
        /// Filter by Routes
        /// </summary>
        public int[]? Routes { get; init; }

        /// <summary>
        /// One map per author: 1: Newest maps, 2: Newest awarded maps
        /// </summary>
        public int? AntiSpam { get; init; }

        /// <summary>
        /// Not used at the moment
        /// </summary>
        [AsNumber]
        public bool? InBeta { get; init; }

        /// <summary>
        /// Map has a custom image (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InScreenshot { get; init; }

        /// <summary>
        /// (Login required) Map is in playlater list (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InPlayLater { get; init; }

        /// <summary>
        /// Only the latest map by author (1) or except (0)
        /// </summary>
        [AsNumber]
        public bool? InLatestAuthor { get; init; }

        /// <summary>
        /// Only the latest awarded map by author (1) or except (0)
        /// </summary>
        [AsNumber]
        public bool? InLatestAwardedAuthor { get; init; }

        /// <summary>
        /// An Author is MX Supporter (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InSupporter { get; init; }

        /// <summary>
        /// (Login required) Map was downloaded (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InDownloads { get; init; }

        /// <summary>
        /// (Login required) Map author is in your favorite user list (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InFavorite { get; init; }

        /// <summary>
        /// A replay driven on map by user specified in replaysby or yourself (login required) (1) and none (0)
        /// </summary>
        [AsNumber]
        public bool? InReplays { get; init; }

        /// <summary>
        /// An online record driven on map by user specified in replaysby or yourself (login required) (1) and none (0)
        /// </summary>
        [AsNumber]
        public bool? InOnlineRecords { get; init; }

        /// <summary>
        /// Map has at least one online record (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InHasRecord { get; init; }

        /// <summary>
        /// Map has at least one replay (1) or none (0)
        /// </summary>
        [AsNumber]
        public bool? InHasReplay { get; init; }

        /// <summary>
        /// Map has been featured on the front page (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InFeatured { get; init; }

        /// <summary>
        /// (TMX only) Map has been Track Of The Day (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InTotd { get; init; }

        /// <summary>
        /// Map is an environment-vehicle mix (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InEnvMix { get; init; }

        /// <summary>
        /// 1: Map has 2 or more co-authors, 0: Map has only one author
        /// </summary>
        [AsNumber]
        public bool? InCollaborative { get; init; }

        /// <summary>
        /// The secret string of a Map, required to access unlisted maps.
        /// </summary>
        public string? Secret { get; init; }

        /// <summary>
        /// The API secret of the mappack, used to access unlisted maps in mappacks with hidden map lists
        /// </summary>
        public long? MappackSecret { get; init; }
    }

    [GetMethod("api/maps")]
    public virtual partial Task<ItemCollection<MapItem>> SearchMapsAsync(SearchMapsParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<ReplayItemMX>]
    public readonly partial record struct SearchReplaysParameters {
        /// <summary>
        /// The MapId of the Map to list Replays for
        /// </summary>
        public required long MapId { get; init; }

        /// <summary>
        /// Amount of results (default: 10)
        /// </summary>
        public int? Count { get; init; }

        /// <summary>
        /// The User.UserId of the driver of the Replay
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// 1: only gets the best replay per user, 0: show only beaten replays (Position is null)
        /// </summary>
        [AsNumber]
        public bool? Best { get; init; }
    }

    [GetMethod("api/replays")]
    public virtual partial Task<ItemCollection<ReplayItemMX>> SearchReplaysAsync(SearchReplaysParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<UserItemMX>]
    public readonly partial record struct SearchUsersParameters {
        /// <summary>
        /// Search order, for order modes refer to Get User Search Orders.
        /// </summary>
        public int? Order1 { get; init; }

        /// <summary>
        /// Secondary search order, for order modes refer to Get User Search Orders.
        /// </summary>
        public int? Order2 { get; init; }

        /// <summary>
        /// Amount of results (default: 40)
        /// </summary>
        public int? Count { get; init; }

        /// <summary>
        /// Display results after specified UserId and given order - used for paging
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Display results before specified UserId and given order - used for paging
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified UserId as well
        /// </summary>
        public long? From { get; init; }

        /// <summary>
        /// Filter by UserId's
        /// </summary>
        public long[]? Id { get; init; }

        /// <summary>
        /// Filter by Name (Full-text)
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Filter by ingame player login associated to the user (exact match)
        /// </summary>
        public string? DriverLogin { get; init; }

        /// <summary>
        /// Filter by min. MapCount
        /// </summary>
        public int? MapsMin { get; init; }

        /// <summary>
        /// Filter by max. MapCount
        /// </summary>
        public int? MapsMax { get; init; }

        /// <summary>
        /// Filter by min. AwardsReceivedCount
        /// </summary>
        public int? AwardsMin { get; init; }

        /// <summary>
        /// Filter by max. AwardsReceivedCount
        /// </summary>
        public int? AwardsMax { get; init; }

        /// <summary>
        /// Filter by min. AwardsGivenCount
        /// </summary>
        public int? AwardsGivenMin { get; init; }

        /// <summary>
        /// Filter by max. AwardsGivenCount
        /// </summary>
        public int? AwardsGivenMax { get; init; }

        /// <summary>
        /// Filter by earliest RegisteredAt (UTC) on the specific site
        /// </summary>
        public DateTimeOffset? RegisteredAfter { get; init; }

        /// <summary>
        /// Filter by latest RegisteredAt (UTC) on the specific site
        /// </summary>
        public DateTimeOffset? RegisteredBefore { get; init; }

        /// <summary>
        /// Users are supporters (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InSupporters { get; init; }

        /// <summary>
        /// Users are moderators (1) or not (0)
        /// </summary>
        [AsNumber]
        public bool? InModerators { get; init; }
    }

    [GetMethod("api/users")]
    public virtual partial Task<ItemCollection<UserItemMX>> SearchUsersAsync(SearchUsersParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<VideoItem>]
    public readonly partial record struct SearchVideosParameters
    {
        // No additional parameters available according to the documentation
    }

    [GetMethod("api/videos")]
    public virtual partial Task<ItemCollection<VideoItem>> SearchVideosAsync(SearchVideosParameters parameters, CancellationToken cancellationToken = default);
    
    [Parameters<MappackItem>]
    public readonly partial record struct SearchMappacksParameters
    {
        /// <summary>
        /// Search order, for order modes refer to Get Mappack Search Orders.
        /// </summary>
        public int? Order1 { get; init; }

        /// <summary>
        /// Secondary search order, for order modes refer to Get Mappack Search Orders.
        /// </summary>
        public int? Order2 { get; init; }

        /// <summary>
        /// Amount of results (default: 40)
        /// </summary>
        public int? Count { get; init; }

        /// <summary>
        /// Display results after specified MappackId and given order - used for paging
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Display results before specified MappackId and given order - used for paging
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified MappackId as well
        /// </summary>
        public long? From { get; init; }

        /// <summary>
        /// Filter by MappackIds
        /// </summary>
        public long[]? Id { get; init; }

        /// <summary>
        /// Filter by Name (Full-text)
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Filter by site username or Name of Managers
        /// </summary>
        public string? Manager { get; init; }

        /// <summary>
        /// Filter by site UserId or UserId of Managers
        /// </summary>
        public long? ManagerUserId { get; init; }

        /// <summary>
        /// Filter by containing Map's MapId
        /// </summary>
        public long? MapId { get; init; }

        /// <summary>
        /// Filter by tags, see Get Tags
        /// </summary>
        public int[]? Tag { get; init; }

        /// <summary>
        /// Filter by excluding tags, see Get Tags
        /// </summary>
        public int[]? ETag { get; init; }

        /// <summary>
        /// true: map must have all specified tags, false: map must have one of the specified tags
        /// </summary>
        [AsNumber] public bool? TagInclusive { get; init; }

        /// <summary>
        /// Filter by min. CreatedAt
        /// </summary>
        public DateTimeOffset? CreatedAfter { get; init; }

        /// <summary>
        /// Filter by max. CreatedAt
        /// </summary>
        public DateTimeOffset? CreatedBefore { get; init; }

        /// <summary>
        /// Filter by Type
        /// </summary>
        public int? PrimaryType { get; init; }

        /// <summary>
        /// Only the latest Mappack by creator (1) or except (0)
        /// </summary>
        [AsNumber] public bool? InLatestAuthor { get; init; }

        /// <summary>
        /// Mappack has a custom image (1) or not (0)
        /// </summary>
        [AsNumber] public bool? InScreenshot { get; init; }

        /// <summary>
        /// A Manager is MX Supporter (1) or not (0)
        /// </summary>
        [AsNumber] public bool? InSupporter { get; init; }

        /// <summary>
        /// (Login required) Mappack manager is in your favorite user list (1) or not (0)
        /// </summary>
        [AsNumber] public bool? InFavorite { get; init; }

        /// <summary>
        /// Mappack has been featured before on the front page (1) or not (0)
        /// </summary>
        [AsNumber] public bool? InFeatured { get; init; }

        /// <summary>
        /// (Login required) Mappack was downloaded (1) or not (0)
        /// </summary>
        [AsNumber] public bool? InDownloads { get; init; }

        /// <summary>
        /// A replay / record driven on any of the mappack maps by any user (1) or none (0)
        /// </summary>
        [AsNumber] public bool? InHasRecords { get; init; }

        /// <summary>
        /// A replay / record driven on mappack map by user specified in lbby or yourself (login required) (1) and none (0)
        /// </summary>
        [AsNumber] public bool? InHasRecord { get; init; }

        /// <summary>
        /// UserId of player to filter for inhasrecord
        /// </summary>
        public long? LbBy { get; init; }

        /// <summary>
        /// The API secret string of a Mappack, required to access hidden map lists of mappacks
        /// </summary>
        public string? MappackSecret { get; init; }
    }

    [GetMethod("api/mappacks")]
    public virtual partial Task<ItemCollection<MappackItem>> SearchMappacksAsync(SearchMappacksParameters parameters, CancellationToken cancellationToken = default);
    
    [GetMethod("api/meta/maporders")]
    public virtual partial Task<SearchOrderItem[]> GetMapSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/userorders")]
    public virtual partial Task<SearchOrderItem[]> GetUserSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/mappackorders")]
    public virtual partial Task<SearchOrderItem[]> GetMappackSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/tags")]
    public virtual partial Task<TagInfo[]> GetTagsAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/maptypes")]
    public virtual partial Task<string[]> GetMaptypesAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/vehicles")]
    public virtual partial Task<string[]> GetVehiclesAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/titlepacks")]
    public virtual partial Task<string[]> GetTitlepacksAsync(CancellationToken cancellationToken = default);

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}