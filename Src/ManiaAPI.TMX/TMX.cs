using ManiaAPI.TMX.Attributes;
using System.Net.Http.Headers;
using System.Text;

namespace ManiaAPI.TMX;

public interface ITMX : IClient
{
    TmxSite Site { get; }
    string SiteName { get; }

    Task<ItemCollection<ReplayItem>> GetReplaysAsync(TMX.GetReplaysParameters parameters, CancellationToken cancellationToken = default);
    Task<ItemCollection<LeaderboardItem>> SearchLeaderboardsAsync(TMX.SearchLeaderboardsParameters parameters, CancellationToken cancellationToken = default);
    Task<ItemCollection<TrackpackItem>> SearchTrackpacksAsync(TMX.SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default);
    Task<ItemCollection<TrackItem>> SearchTracksAsync(TMX.SearchTracksParameters parameters, CancellationToken cancellationToken = default);
    Task<ItemCollection<UserItem>> SearchUsersAsync(TMX.SearchUsersParameters parameters, CancellationToken cancellationToken = default);
    Task<ItemCollection<VideoItem>> SearchVideosAsync(TMX.SearchVideosParameters parameters, CancellationToken cancellationToken = default);

    Task<SearchOrderItem[]> GetTrackSearchOrdersAsync(CancellationToken cancellationToken = default);
    Task<SearchOrderItem[]> GetTrackpackSearchOrdersAsync(CancellationToken cancellationToken = default);
    Task<SearchOrderItem[]> GetUserSearchOrdersAsync(CancellationToken cancellationToken = default);
    Task<SearchOrderItem[]> GetVideoSearchOrdersAsync(CancellationToken cancellationToken = default);
    Task<SearchOrderItem[]> GetLeaderboardSearchOrdersAsync(CancellationToken cancellationToken = default);
    Task<TagInfo[]> GetTagsAsync(CancellationToken cancellationToken = default);
    Task<TagDescription[]> GetTagDescriptionsAsync(CancellationToken cancellationToken = default);
    Task<string[]> GetTrackTypesAsync(CancellationToken cancellationToken = default);
    Task<string[]> GetVehiclesAsync(CancellationToken cancellationToken = default);

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

    string GetTrackRandomUrl();
    Task<long?> GetRandomTrackIdAsync(TMX.SearchTracksParameters parameters, CancellationToken cancellationToken = default);
    Task<TrackItem?> GetRandomTrackAsync(TMX.SearchTracksParameters parameters, CancellationToken cancellationToken = default);

    string GetTrackpackRandomUrl();
    Task<long?> GetRandomTrackpackIdAsync(TMX.SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default);
    Task<TrackpackItem?> GetRandomTrackpackAsync(TMX.SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default);
}

public partial class TMX : ITMX
{
    public HttpClient Client { get; }
    public TmxSite Site { get; }
    public string SiteName { get; }

    public TMX(HttpClient client, TmxSite site)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(TMX; Discord=bigbang1112)"));
        }

        Site = site;
        SiteName = site.ToString();

        var url = site switch
        {
            TmxSite.TMUF => "https://tmuf.exchange/",
            TmxSite.TMNF => "https://tmnf.exchange/",
            TmxSite.Original => "https://original.tm-exchange.com/",
            TmxSite.Sunrise => "https://sunrise.tm-exchange.com/",
            TmxSite.Nations => "https://nations.tm-exchange.com/",
            _ => throw new NotImplementedException()
        };

        Client.BaseAddress = new Uri(url);
    }

    public TMX(TmxSite site) : this(new HttpClient(), site) { }

    [Parameters<ReplayItem>]
    public readonly partial record struct GetReplaysParameters
    {
        public required long TrackId { get; init; }
        public int? Count { get; init; }

        /// <summary>
        /// After a certain replay ID.
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Before a certain replay ID.
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified replay ID as well.
        /// </summary>
        public long? From { get; init; }

        [AsNumber] public bool? Best { get; init; }
        public long? UserId { get; init; }

        public IEnumerable<KeyValuePair<string, string>>? AdditionalParameters { get; init; }
    }

    [GetMethod("api/replays")]
    public virtual partial Task<ItemCollection<ReplayItem>> GetReplaysAsync(GetReplaysParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<TrackItem>]
    public readonly partial record struct SearchTracksParameters
    {
        public TrackOrder? Order1 { get; init; }
        public int? Count { get; init; }

        /// <summary>
        /// After a certain track ID.
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Before a certain track ID.
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified track ID as well.
        /// </summary>
        public long? From { get; init; }

        public long[]? Id { get; init; }
        public string[]? UId { get; init; }
        public string? Name { get; init; }
        public string? Author { get; init; }
        public long? AuthorUserId { get; init; }
        public long? VideoId { get; init; }
        public long? PackId { get; init; }
        public string? AwardedBy { get; init; }
        public string? CommentedBy { get; init; }
        public TrackStyle[]? Tag { get; init; }
        public TrackStyle[]? ETag { get; init; }
        public bool? TagInclusive { get; init; }
        public TrackType? PrimaryType { get; init; }
        public LeaderboardType? LbType { get; init; }
        public DateTimeOffset? UploadedAfter { get; init; }
        public DateTimeOffset? UploadedBefore { get; init; }
        public int? AuthorTimeMin { get; init; }
        public int? AuthorTimeMax { get; init; }
        public Environment[]? Environment { get; init; }
        public Environment[]? Vehicle { get; init; }
        public Mood[]? Mood { get; init; }
        public Difficulty[]? Difficulty { get; init; }
        public TrackRoutes[]? Routes { get; init; }
        public int? AntiSpam { get; init; }
        [AsNumber] public bool? InScreenshot { get; init; }
        [AsNumber] public bool? InLatestAuthor { get; init; }
        [AsNumber] public bool? InLatestAwardedAuthor { get; init; }
        [AsNumber] public bool? InSupporter { get; init; }
        [AsNumber] public bool? InHasRecord { get; init; }
        [AsNumber] public bool? InEnvMix { get; init; }
        [AsNumber] public bool? InUnlimiter { get; init; }
        [AsNumber] public bool? InAuthorTimeBeaten { get; init; }
        [AsNumber] public bool? InCollaborative { get; init; }
        [AsNumber] public bool? InBeta { get; init; }
        public TrackOrder? Order2 { get; init; } // no longer?

        public IEnumerable<KeyValuePair<string, string>>? AdditionalParameters { get; init; }
    }

    [GetMethod("api/tracks")]
    public virtual partial Task<ItemCollection<TrackItem>> SearchTracksAsync(SearchTracksParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<LeaderboardItem>]
    public readonly partial record struct SearchLeaderboardsParameters
    {
        public int? Order1 { get; init; }
        public int? Count { get; init; }

        /// <summary>
        /// After a certain user ID.
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Before a certain user ID.
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified user ID as well.
        /// </summary>
        public long? From { get; init; }
        public int? LbId { get; init; }
        public int? LbEnv { get; init; }

        public IEnumerable<KeyValuePair<string, string>>? AdditionalParameters { get; init; }
    }

    [GetMethod("api/leaderboards")]
    public virtual partial Task<ItemCollection<LeaderboardItem>> SearchLeaderboardsAsync(SearchLeaderboardsParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<TrackpackItem>]
    public readonly partial record struct SearchTrackpacksParameters
    {
        public int? Order1 { get; init; }
        public int? Count { get; init; }

        /// <summary>
        /// After a certain trackpack ID.
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Before a certain trackpack ID.
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified trackpack ID as well.
        /// </summary>
        public long? From { get; init; }

        public long[]? Id { get; init; }
        public string? Name { get; init; }
        public string? Creator { get; init; }

        public IEnumerable<KeyValuePair<string, string>>? AdditionalParameters { get; init; }
    }

    [GetMethod("api/trackpacks")]
    public virtual partial Task<ItemCollection<TrackpackItem>> SearchTrackpacksAsync(SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<UserItem>]
    public readonly partial record struct SearchUsersParameters
    {
        public int? Order1 { get; init; }
        public int? Count { get; init; }

        /// <summary>
        /// After a certain user ID.
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Before a certain user ID.
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified user ID as well.
        /// </summary>
        public long? From { get; init; }

        public long[]? Id { get; init; }
        public string? Name { get; init; }
        public long[]? MxId { get; init; }
        public int? TracksMin { get; init; }
        public int? TracksMax { get; init; }
        public int? AwardsMin { get; init; }
        public int? AwardsMax { get; init; }
        public int? AwardsGivenMin { get; init; }
        public int? AwardsGivenMax { get; init; }
        public DateTimeOffset? RegisteredAfter { get; init; }
        public DateTimeOffset? RegisteredBefore { get; init; }
        [AsNumber] public bool? InSupporters { get; init; }
        [AsNumber] public bool? InModerators { get; init; }

        public IEnumerable<KeyValuePair<string, string>>? AdditionalParameters { get; init; }
    }

    [GetMethod("api/users")]
    public virtual partial Task<ItemCollection<UserItem>> SearchUsersAsync(SearchUsersParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<VideoItem>]
    public readonly partial record struct SearchVideosParameters
    {
        /// <summary>
        /// Search order, for order modes refer to <see cref="GetVideoSearchOrdersAsync(CancellationToken)"/>.
        /// </summary>
        public int? Order1 { get; init; }

        /// <summary>
        /// Amount of results (default: 40).
        /// </summary>
        public int? Count { get; init; }

        /// <summary>
        /// After a certain video ID.
        /// </summary>
        public long? After { get; init; }

        /// <summary>
        /// Before a certain video ID.
        /// </summary>
        public long? Before { get; init; }

        /// <summary>
        /// Like <see cref="After"/>, but includes the specified video ID as well.
        /// </summary>
        public long? From { get; init; }

        /// <summary>
        /// Filter by video IDs.
        /// </summary>
        public long[]? Id { get; init; }

        /// <summary>
        /// Filter out video IDs.
        /// </summary>
        public long[]? NId { get; init; }

        /// <summary>
        /// Filter by IDs of tracks featured in the video.
        /// </summary>
        public long[]? TrackId { get; init; }

        /// <summary>
        /// Filter by video title.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Filter by video author (YouTube channel name).
        /// </summary>
        public string? Author { get; init; }

        /// <summary>
        /// Filter by the site username of the posting user.
        /// </summary>
        public string? Poster { get; init; }

        /// <summary>
        /// Filter by the site username of the creating user.
        /// </summary>
        public string? Creator { get; init; }

        public IEnumerable<KeyValuePair<string, string>>? AdditionalParameters { get; init; }
    }

    [GetMethod("api/videos")]
    public virtual partial Task<ItemCollection<VideoItem>> SearchVideosAsync(SearchVideosParameters parameters, CancellationToken cancellationToken = default);

    [GetMethod("api/meta/trackorders")]
    public virtual partial Task<SearchOrderItem[]> GetTrackSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/trackpackorders")]
    public virtual partial Task<SearchOrderItem[]> GetTrackpackSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/userorders")]
    public virtual partial Task<SearchOrderItem[]> GetUserSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/videoorders")]
    public virtual partial Task<SearchOrderItem[]> GetVideoSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/leaderboardorders")]
    public virtual partial Task<SearchOrderItem[]> GetLeaderboardSearchOrdersAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/tags")]
    public virtual partial Task<TagInfo[]> GetTagsAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/tagdescriptions")]
    public virtual partial Task<TagDescription[]> GetTagDescriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the track types; the index of each is the value used in search filters.
    /// </summary>
    [GetMethod("api/meta/tracktypes")]
    public virtual partial Task<string[]> GetTrackTypesAsync(CancellationToken cancellationToken = default);

    [GetMethod("api/meta/vehicles")]
    public virtual partial Task<string[]> GetVehiclesAsync(CancellationToken cancellationToken = default);

    public string GetTrackRandomUrl() => $"{Client.BaseAddress}trackrandom";

    public async Task<long?> GetRandomTrackIdAsync(SearchTracksParameters parameters, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("trackrandom");
        parameters.AppendQueryString(sb, appendFields: false);

        using var request = new HttpRequestMessage(HttpMethod.Head, sb.ToString());
        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        if (long.TryParse(response.RequestMessage?.RequestUri?.Segments.LastOrDefault(), out var trackId))
        {
            return trackId;
        }

        return null;
    }

    public async Task<TrackItem?> GetRandomTrackAsync(SearchTracksParameters parameters, CancellationToken cancellationToken = default)
    {
        var trackId = await GetRandomTrackIdAsync(parameters, cancellationToken);

        if (!trackId.HasValue)
        {
            return null;
        }

        var tracks = await SearchTracksAsync(new SearchTracksParameters
        {
            Id = [trackId.Value],
            Count = 1
        }, cancellationToken);

        return tracks.Results.FirstOrDefault();
    }

    public string GetTrackpackRandomUrl() => $"{Client.BaseAddress}trackpackrandom";

    public async Task<long?> GetRandomTrackpackIdAsync(SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("trackpackrandom");
        parameters.AppendQueryString(sb, appendFields: false);

        using var request = new HttpRequestMessage(HttpMethod.Head, sb.ToString());
        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        if (long.TryParse(response.RequestMessage?.RequestUri?.Segments.LastOrDefault(), out var trackpackId))
        {
            return trackpackId;
        }

        return null;
    }

    public async Task<TrackpackItem?> GetRandomTrackpackAsync(SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default)
    {
        var trackpackId = await GetRandomTrackpackIdAsync(parameters, cancellationToken);

        if (!trackpackId.HasValue)
        {
            return null;
        }

        var trackpacks = await SearchTrackpacksAsync(new SearchTrackpacksParameters
        {
            Id = [trackpackId.Value],
            Count = 1
        }, cancellationToken);

        return trackpacks.Results.FirstOrDefault();
    }

    public string GetReplayGbxUrl(long replayId) => $"{Client.BaseAddress}recordgbx/{replayId}";
    public string GetReplayGbxUrl(ReplayItem replay) => GetReplayGbxUrl(replay.ReplayId);

    public virtual async Task<HttpResponseMessage> GetReplayGbxResponseAsync(long replayId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetReplayGbxUrl(replayId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetReplayGbxResponseAsync(ReplayItem replay, CancellationToken cancellationToken = default)
    {
        return await GetReplayGbxResponseAsync(replay.ReplayId, cancellationToken);
    }

    public string GetTrackGbxUrl(long trackId) => $"{Client.BaseAddress}trackgbx/{trackId}";
    public string GetTrackGbxUrl(TrackItem track) => GetTrackGbxUrl(track.TrackId);

    public virtual async Task<HttpResponseMessage> GetTrackGbxResponseAsync(long trackId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetTrackGbxUrl(trackId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetTrackGbxResponseAsync(TrackItem track, CancellationToken cancellationToken = default)
    {
        return await GetTrackGbxResponseAsync(track.TrackId, cancellationToken);
    }

    public string GetTrackThumbnailUrl(long trackId) => $"{Client.BaseAddress}trackshow/{trackId}/image/0";
    public string GetTrackThumbnailUrl(TrackItem track) => GetTrackThumbnailUrl(track.TrackId);

    public virtual async Task<HttpResponseMessage> GetTrackThumbnailResponseAsync(long trackId, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetTrackThumbnailUrl(trackId), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetTrackThumbnailResponseAsync(TrackItem track, CancellationToken cancellationToken = default)
    {
        return await GetTrackThumbnailResponseAsync(track.TrackId, cancellationToken);
    }

    public string GetTrackImageUrl(long trackId, int imageIndex) => $"{Client.BaseAddress}trackshow/{trackId}/image/{imageIndex}";
    public string GetTrackImageUrl(TrackItem track, int imageIndex) => GetTrackImageUrl(track.TrackId, imageIndex);

    public virtual async Task<HttpResponseMessage> GetTrackImageResponseAsync(long trackId, int imageIndex, CancellationToken cancellationToken = default)
    {
        return await Client.GetAsync(GetTrackImageUrl(trackId, imageIndex), cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> GetTrackImageResponseAsync(TrackItem track, int imageIndex, CancellationToken cancellationToken = default)
    {
        return await GetTrackImageResponseAsync(track.TrackId, imageIndex, cancellationToken);
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
