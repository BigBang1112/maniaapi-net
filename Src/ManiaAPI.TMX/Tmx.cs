using ManiaAPI.Base;
using ManiaAPI.Base.Converters;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ManiaAPI.TMX;

public class TMX : JsonAPI
{
    private static readonly Dictionary<Type, string> fieldQueryStrings = new();

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

        route += "fields=" + GetFieldsQuery<TrackSearchItem>();

        return await GetApiAsync<ItemCollection<TrackSearchItem>>(route, cancellationToken);
    }

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(TrackSearchItem track, CancellationToken cancellationToken = default)
    {
        return await GetReplaysAsync(track.TrackId, cancellationToken);
    }

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(int trackId, CancellationToken cancellationToken = default)
    {
        var route = $"replays?trackid={trackId}&count=1000";
        
        route += "&fields=" + GetFieldsQuery<ReplayItem>();

        return await base.GetApiAsync<ItemCollection<ReplayItem>>(route, cancellationToken);
    }

    private static IEnumerable<string> GetFields(Type type)
    {
        foreach (var prop in type.GetProperties())
        {
            var name = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;

            if (prop.PropertyType.IsArray)
            {
                name += "[]";
            }

            var hadInnerFields = false;

            // Is property type is record
            if (prop.PropertyType.GetMethod("<Clone>$") is not null)
            {
                foreach (var innerField in GetFields(prop.PropertyType))
                {
                    yield return $"{name}.{innerField}";
                    hadInnerFields = true;
                }
            }

            if (!hadInnerFields)
            {
                yield return name;
            }
        }
    }

    private static string GetFieldsQuery<T>()
    {
        var type = typeof(T);

        if (fieldQueryStrings.TryGetValue(type, out string? query))
        {
            return query;
        }

        query = WebUtility.UrlEncode(string.Join(',', GetFields(type)));

        fieldQueryStrings.Add(type, query);

        return query;
    }
}
