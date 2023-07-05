using ManiaAPI.TMX.Attributes;
using ManiaAPI.TMX.JsonContexts;

namespace ManiaAPI.TMX;

public partial class TMX : IClient
{
    public HttpClient Client { get; }
    public TmxSite Site { get; }
    public string SiteName { get; }

    public TMX(HttpClient client, TmxSite site)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TMX) by BigBang1112");

        Site = site;
        SiteName = site.ToStringFast();

        var url = site switch
        {
            TmxSite.TMUF => "https://tmuf.exchange/api/",
            TmxSite.TMNF => "https://tmnf.exchange/api/",
            TmxSite.Original => "https://original.tm-exchange.com/api/",
            TmxSite.Sunrise => "https://sunrise.tm-exchange.com/api/",
            TmxSite.Nations => "https://nations.tm-exchange.com/api/",
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
        public long? After { get; init; }
        public long? Before { get; init; }
        public long? From { get; init; }
        [AsNumber] public bool? Best { get; init; }
        public long? UserId { get; init; }
    }

    [GetMethod<ItemCollection_ReplayItem>("replays")]
    public partial Task<ItemCollection<ReplayItem>> GetReplaysAsync(GetReplaysParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<TrackItem>]
    public readonly partial record struct SearchTracksParameters
    {
        public int? Count { get; init; }
        public string? Author { get; init; }
        public Environment? Environment { get; init; }
        public Environment? Vehicle { get; init; }
        public TrackType? PrimaryType { get; init; }
        public TrackStyle? Tag { get; init; }
        public Mood? Mood { get; init; }
        public Difficulty? Difficulty { get; init; }
        public TrackRoutes? Routes { get; init; }
        public LeaderboardType? LbType { get; init; }
        public TrackOrder? Order1 { get; init; }
        public TrackOrder? Order2 { get; init; }

        /// <summary>
        /// After a certain track ID.
        /// </summary>
        public int? After { get; init; }

        /// <summary>
        /// Before a certain track ID.
        /// </summary>
        public int? Before { get; init; }
    }

    [GetMethod<ItemCollection_TrackItem>("tracks")]
    public partial Task<ItemCollection<TrackItem>> SearchTracksAsync(SearchTracksParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<LeaderboardItem>]
    public readonly partial record struct SearchLeaderboardsParameters
    {
        public int? Order1 { get; init; }
        public int? Count { get; init; }
        public long? After { get; init; }
        public long? Before { get; init; }
        public long? From { get; init; }
        public int? LbId { get; init; }
        public int? LbEnv { get; init; }
    }

    [GetMethod<ItemCollection_LeaderboardItem>("leaderboards")]
    public partial Task<ItemCollection<LeaderboardItem>> SearchLeaderboardsAsync(SearchLeaderboardsParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<TrackpackItem>]
    public readonly partial record struct SearchTrackpacksParameters
    {
        public int? Order1 { get; init; }
        public int? Count { get; init; }
        public long? After { get; init; }
        public long? Before { get; init; }
        public long? From { get; init; }
        public long[]? Id { get; init; }
        public string? Name { get; init; }
        public string? Creator { get; init; }
    }

    [GetMethod<ItemCollection_TrackpackItem>("trackpacks")]
    public partial Task<ItemCollection<TrackpackItem>> SearchTrackpacksAsync(SearchTrackpacksParameters parameters, CancellationToken cancellationToken = default);

    [Parameters<UserItem>]
    public readonly partial record struct SearchUsersParameters
    {
        public int? Order1 { get; init; }
        public int? Count { get; init; }
        public long? After { get; init; }
        public long? Before { get; init; }
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

    [GetMethod<ItemCollection_UserItem>("users")]
    public partial Task<ItemCollection<UserItem>> SearchUsersAsync(SearchUsersParameters parameters, CancellationToken cancellationToken = default);

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
