using ManiaAPI.TMX.Converters;
using ManiaAPI.TMX.JsonContexts;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ManiaAPI.TMX;

public class TMX
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

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(GetReplaysParameters parameters, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("replays?");
        parameters.AppendQueryString(sb);

        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ItemCollection_ReplayItem.TypeInfo, cancellationToken) ?? new();
    }

    public readonly record struct GetReplaysParameters
    {
        public required long TrackId { get; init; }
        public int? Count { get; init; }
        public long? After { get; init; }
        public long? Before { get; init; }
        public long? From { get; init; }
        public bool? Best { get; init; }
        public long? UserId { get; init; }
        public ReplayItemFields Fields { get; init; }

        public GetReplaysParameters()
        {
            Fields = ReplayItemFields.All;
        }

        internal void AppendQueryString(StringBuilder sb)
        {
            sb.Append("trackid=");
            sb.Append(TrackId);

            if (Count.HasValue)
            {
                sb.Append("&count=");
                sb.Append(Count.Value);
            }

            if (After.HasValue)
            {
                sb.Append("&after=");
                sb.Append(After.Value);
            }

            if (Before.HasValue)
            {
                sb.Append("&before=");
                sb.Append(Before.Value);
            }

            if (From.HasValue)
            {
                sb.Append("&from=");
                sb.Append(From.Value);
            }

            if (Best.HasValue)
            {
                sb.Append("&best=");
                sb.Append(Best.Value ? '1' : '0');
            }

            if (UserId.HasValue)
            {
                sb.Append("&userid=");
                sb.Append(UserId.Value);
            }

            sb.Append("&fields=");
            Fields.Append(sb);
        }   
    }

    public readonly record struct ReplayItemFields
    {
        public bool ReplayId { get; init; }
        public UserFields User { get; init; }
        public bool ReplayTime { get; init; }
        public bool ReplayScore { get; init; }
        public bool ReplayRespawns { get; init; }
        public bool Score { get; init; }
        public bool Position { get; init; }
        public bool IsBest { get; init; }
        public bool IsLeaderboard { get; init; }
        public bool TrackAt { get; init; }
        public bool ReplayAt { get; init; }

        public static readonly ReplayItemFields All = new()
        {
            ReplayId = true,
            User = UserFields.All,
            ReplayTime = true,
            ReplayScore = true,
            ReplayRespawns = true,
            Score = true,
            Position = true,
            IsBest = true,
            IsLeaderboard = true,
            TrackAt = true,
            ReplayAt = true
        };

        internal void Append(StringBuilder sb)
        {
            var first = true;

            if (ReplayId)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(ReplayId));
                first = false;
            }

            if (User.UserId)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(User));
                sb.Append('.');
                sb.Append(nameof(ManiaAPI.TMX.User.UserId));
                first = false;
            }

            if (User.Name)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(User));
                sb.Append('.');
                sb.Append(nameof(ManiaAPI.TMX.User.Name));
            }

            if (ReplayTime)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(ReplayTime));
                first = false;
            }

            if (ReplayScore)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(ReplayScore));
                first = false;
            }

            if (ReplayRespawns)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(ReplayRespawns));
                first = false;
            }

            if (Score)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(Score));
                first = false;
            }

            if (Position)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(Position));
                first = false;
            }

            if (IsBest)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(IsBest));
                first = false;
            }

            if (IsLeaderboard)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(IsLeaderboard));
                first = false;
            }

            if (TrackAt)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(TrackAt));
                first = false;
            }

            if (ReplayAt)
            {
                if (!first) sb.Append(',');
                sb.Append(nameof(ReplayAt));
            }
        }
    }

    public Task<ItemCollection<TrackSearchItem>> SearchTracksAsync(TrackSearchFilters? filters = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
