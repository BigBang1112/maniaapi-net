using ManiaAPI.TMX.Attributes;
using ManiaAPI.TMX.Converters;
using ManiaAPI.TMX.JsonContexts;
using System.Net.Http.Json;
using System.Text;

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

        Client.BaseAddress = new Uri($"https://{SiteName}.exchange/api/");
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
}
