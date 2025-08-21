using ManiaAPI.TMX.Attributes;

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
}

public partial class TMX : ITMX
{
    public HttpClient Client { get; }
    public TmxSite Site { get; }
    public string SiteName { get; }

    public TMX(HttpClient client, TmxSite site)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NET/2.3.1 (TMX; Discord=bigbang1112)");

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
        public TrackOrder? Order2 { get; init; } // no longer?
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
    }

    [GetMethod("api/users")]
    public virtual partial Task<ItemCollection<UserItem>> SearchUsersAsync(SearchUsersParameters parameters, CancellationToken cancellationToken = default);

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
