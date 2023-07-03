using ManiaAPI.TMX.Attributes;
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

    [Parameters<ReplayItem>] // source generate Fields property and stuff below
    public readonly partial record struct GetReplaysParameters
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

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(GetReplaysParameters parameters, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("replays?");
        parameters.AppendQueryString(sb);

        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ItemCollection_ReplayItem.TypeInfo, cancellationToken) ?? new();
    }

    public Task<ItemCollection<TrackSearchItem>> SearchTracksAsync(TrackSearchFilters? filters = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
