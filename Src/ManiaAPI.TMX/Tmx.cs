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

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(GetReplaysParameters parameters, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("replays");
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
