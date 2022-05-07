using ManiaAPI.Base;
using ManiaAPI.Base.Converters;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManiaAPI.TMX;

public class TMX : JsonAPI
{
    private const string ApiRoute = "api";
    private const string BaseUrl = "https://{0}.tm-exchange.com";
    private const string BaseApiUrl = $"{BaseUrl}/{ApiRoute}/";

    public TmxSite Site { get; }

    public string SiteName { get; }

    static TMX()
    {
        JsonSerializerOptions.Converters.Add(new DateTimeUtcFixConverter());
    }

    public TMX(TmxSite site) : base(string.Format(BaseApiUrl, site.ToString().ToLower()), automaticallyAuthorize: true)
    {
        Site = site;
        SiteName = site.ToString();

        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TMX) by BigBang1112");
    }

    public async Task<ItemCollection<TrackSearchItem>> SearchAsync(TrackSearchFilters? filters = default, CancellationToken cancellationToken = default)
    {
        var route = "tracks";

        if (filters is not null)
        {
            route += filters.ToQuery();
        }

        return await GetApiAsync<ItemCollection<TrackSearchItem>>(route, cancellationToken);
    }

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(TrackSearchItem track, CancellationToken cancellationToken = default)
    {
        return await GetReplaysAsync(track.TrackId, cancellationToken);
    }

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(int trackId, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<ItemCollection<ReplayItem>>($"replays?trackid={trackId}&count=1000", cancellationToken);
    }
}
