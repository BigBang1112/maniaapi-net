using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ManiaAPI.ManiaPlanetAPI;

public interface IManiaPlanetIngameAPI
{
    HttpClient Client { get; }

    Task<ManiaPlanetIngameAuthResponse> AuthenticateAsync(string login, string token, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DownloadTitleAsync(HttpMethod method, string titleId, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DownloadTitleAsync(string titleId, CancellationToken cancellationToken = default);
    Task<IngameTitle?> GetTitleByUidAsync(string uid, CancellationToken cancellationToken = default);
}

public class ManiaPlanetIngameAPI : IManiaPlanetIngameAPI
{
    public const string BaseAddress = "https://prod.live.maniaplanet.com/ingame/";

    public HttpClient Client { get; }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet ingame API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public ManiaPlanetIngameAPI(HttpClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = "2.7.0";

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(ManiaPlanetIngameAPI; Email=petrpiv1@gmail.com; Discord=bigbang1112)"));
        }
    }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet ingame API client.
    /// </summary>
    public ManiaPlanetIngameAPI() : this(new HttpClient { BaseAddress = new Uri(BaseAddress) }) { }

    public async Task<ManiaPlanetIngameAuthResponse> AuthenticateAsync(string login, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(login);
        ArgumentException.ThrowIfNullOrEmpty(token);

        using var request = new HttpRequestMessage(HttpMethod.Get, "auth");
        request.Headers.Add("Maniaplanet-Auth", $"Login=\"{login}\", Token=\"{token}\"");
        request.Headers.Accept.Add(new("application/json"));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ManiaPlanetIngameAPIJsonContext.Default.ManiaPlanetIngameAuthResponse, cancellationToken) ?? throw new Exception("This should never happen");
    }

    public async Task<HttpResponseMessage> DownloadTitleAsync(string titleId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(titleId);

        return await DownloadTitleAsync(HttpMethod.Get, titleId, cancellationToken);
    }

    public async Task<HttpResponseMessage> DownloadTitleAsync(HttpMethod method, string titleId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(titleId);

        using var request = new HttpRequestMessage(method, $"public/titles/download/{titleId}.Title.Pack.gbx");
        return await Client.SendAsync(request, cancellationToken);
    }

    public virtual async Task<IngameTitle?> GetTitleByUidAsync(string uid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(uid);

        using var response = await Client.GetAsync($"public/titles/{uid}", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ManiaPlanetIngameAPIJsonContext.Default.IngameTitle, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filters">Possible values: shootmania, trackmania, solo, multiplayer, matchmaking, environments</param>
    /// <param name="orderBy">Possible values: onlinePlayers, lastUpdate, registrations, playersLast24h</param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ImmutableList<IngameTitle>> SearchTitlesAsync(
        string[]? filters = null,
        string? orderBy = "onlinePlayers",
        int offset = 0,
        int length = 10,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("public/titles");
        var first = true;

        if (filters is not null)
        {
            foreach (var filter in filters)
            {
                sb.Append(first ? '?' : '&');
                first = false;

                sb.Append("filters%5b%5d=");
                sb.Append(filter);
            }
        }

        if (orderBy is not null and not "onlinePlayers")
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append($"orderBy=");
            sb.Append(orderBy);
        }

        if (offset != 0)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append($"offset=");
            sb.Append(offset);
        }

        if (length != 10)
        {
            sb.Append(first ? '?' : '&');

            sb.Append($"length=");
            sb.Append(length);
        }

        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ManiaPlanetIngameAPIJsonContext.Default.ImmutableListIngameTitle, cancellationToken)
            ?? throw new Exception("This shouldn't be null.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="login"></param>
    /// <param name="token"></param>
    /// <param name="orderBy">Possible values: playerCount, levelClass1</param>
    /// <param name="titleUids"></param>
    /// <param name="environments"></param>
    /// <param name="scriptName"></param>
    /// <param name="search"></param>
    /// <param name="zone"></param>
    /// <param name="onlyPublic"></param>
    /// <param name="onlyPrivate"></param>
    /// <param name="onlyFavorite"></param>
    /// <param name="onlyLobby"></param>
    /// <param name="excludeLobby"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ImmutableList<OnlineServer>> SearchOnlineServersAsync(
        string login,
        string token,
        string orderBy = "playerCount",
        string[]? titleUids = null,
        string[]? environments = null,
        string? scriptName = null,
        string? search = null,
        string? zone = null,
        bool onlyPublic = false,
        bool onlyPrivate = false,
        bool onlyFavorite = false,
        bool onlyLobby = false,
        bool excludeLobby = true,
        int offset = 0,
        int length = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(login);
        ArgumentException.ThrowIfNullOrEmpty(token);

        var sb = new StringBuilder("servers/online");

        var first = true;

        if (orderBy is not null and not "playerCount")
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append($"orderBy=");
            sb.Append(orderBy);
        }

        if (titleUids is not null)
        {
            foreach (var titleUid in titleUids)
            {
                sb.Append(first ? '?' : '&');
                first = false;

                sb.Append("titleUids[]=");
                sb.Append(titleUid);
            }
        }

        if (environments is not null)
        {
            foreach (var environment in environments)
            {
                sb.Append(first ? '?' : '&');
                first = false;

                sb.Append("environments[]=");
                sb.Append(environment);
            }
        }

        if (scriptName is not null)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("scriptName=");
            sb.Append(scriptName);
        }

        if (search is not null)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("search=");
            sb.Append(search);
        }

        if (zone is not null)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("zone=");
            sb.Append(zone);
        }

        if (onlyPublic)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("onlyPublic=1");
        }

        if (onlyPrivate)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("onlyPrivate=1");
        }

        if (onlyFavorite)
        {
            sb.Append(first ? '?' : '&');
            first = false;
            sb.Append("onlyFavorite=1");
        }

        if (onlyLobby)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("onlyLobby=1");
        }

        if (!excludeLobby)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("excludeLobby=0");
        }

        if (offset != 0)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("offset=");
            sb.Append(offset);
        }

        if (length != 10)
        {
            sb.Append(first ? '?' : '&');

            sb.Append("length=");
            sb.Append(length);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());
        request.Headers.Add("Maniaplanet-Auth", $"Login=\"{login}\", Token=\"{token}\"");
        request.Headers.Accept.Add(new("application/json"));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ManiaPlanetAPIJsonContext.Default.ImmutableListOnlineServer, cancellationToken)
            ?? throw new Exception("This shouldn't be null.");
    }
}
