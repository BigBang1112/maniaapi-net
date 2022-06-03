using ManiaAPI.Base;
using ManiaAPI.Base.Converters;

namespace ManiaAPI.TMX;

public class TMX : JsonAPI
{
    private const string ApiRoute = "api";
    private const string BaseUrl = "https://{0}.tm-exchange.com";
    private const string BaseApiUrl = $"{BaseUrl}/{ApiRoute}/";

    public TmxSite Site { get; }

    public string SiteName { get; }

    public TMX(TmxSite site) : base(string.Format(BaseApiUrl, site.ToString().ToLower()), automaticallyAuthorize: true)
    {
        Site = site;
        SiteName = site.ToString();

        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TMX) by BigBang1112");

        JsonSerializerOptionsInObject.Converters.Add(new DateTimeUtcFixConverter());
    }

    public async Task<ItemCollection<TrackSearchItem>> SearchAsync(TrackSearchFilters? filters = default, CancellationToken cancellationToken = default)
    {
        var route = "tracks";

        if (filters is not null)
        {
            route += filters.ToQuery() + "&";
        }
        else
        {
            route += "?";
        }

        route += "fields=TrackId%2CTrackName%2CAuthors%5B%5D%2CTags%5B%5D%2CAuthorTime%2CRoutes%2CDifficulty%2CEnvironment%2CCar%2CPrimaryType%2CMood%2CAwards%2CHasThumbnail%2CImages%5B%5D%2CIsPublic";

        return await GetApiAsync<ItemCollection<TrackSearchItem>>(route, cancellationToken);
    }

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(TrackSearchItem track, CancellationToken cancellationToken = default)
    {
        return await GetReplaysAsync(track.TrackId, cancellationToken);
    }

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(int trackId, CancellationToken cancellationToken = default)
    {
        var route = $"replays?trackid={trackId}&count=1000";
        
        route += "&fields=ReplayId%2CUser.UserId%2CUser.Name%2CReplayTime%2CReplayScore%2CReplayRespawns%2CTrackAt%2CValidated%2CReplayAt%2CScore";
        
        return await base.GetApiAsync<ItemCollection<ReplayItem>>(route, cancellationToken);
    }
}
