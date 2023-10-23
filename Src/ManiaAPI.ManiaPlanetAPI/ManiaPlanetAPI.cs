using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using ManiaAPI.ManiaPlanetAPI.JsonContexts;
using System.Text;

namespace ManiaAPI.ManiaPlanetAPI;

public interface IManiaPlanetAPI : IDisposable
{
    Task AuthorizeAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);
    Task AuthorizeAsync(string clientId, string clientSecret, string[] scopes, CancellationToken cancellationToken = default);
    Task<DedicatedAccount[]> GetDedicatedAccountsAsync(CancellationToken cancellationToken = default);
    Task<string> GetEmailAsync(CancellationToken cancellationToken = default);
    Task<Map[]> GetMapsAsync(CancellationToken cancellationToken = default);
    Task<Player> GetPlayerAsync(CancellationToken cancellationToken = default);
    Task<OnlineServer[]> GetOnlineServersAsync(string orderBy = "playerCount", string[]? titleUids = null, string[]? environments = null, string? scriptName = null, string? search = null, string? zone = null, bool onlyPublic = false, bool onlyPrivate = false, bool onlyLobby = false, bool excludeLobby = true, int offset = 0, int length = 10, CancellationToken cancellationToken = default);
    Task<Title?> GetTitleByUidAsync(string uid, CancellationToken cancellationToken = default);
    Task<Title[]> GetTitlesAsync(CancellationToken cancellationToken = default);
    Task<Title[]> GetTitlesAsync(string[]? filters = null, string? orderBy = "onlinePlayers", int offset = 0, int length = 10, CancellationToken cancellationToken = default);
    Task<TitleScript[]> GetTitleScriptsAsync(string uid, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetZonesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// ManiaPlanet WebServices API client.
/// </summary>
public class ManiaPlanetAPI : IManiaPlanetAPI
{
    private string? accessToken;
    private string? clientId;
    private string? clientSecret;
    private AuthenticationHeaderValue? authorization;

    public const string BaseAddress = "https://maniaplanet.com/webservices/";

    public JwtPayloadManiaPlanetAPI? Payload { get; private set; }

    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    public HttpClient Client { get; }

    /// <summary>
    /// If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached.
    /// This is only considered after calling <see cref="AuthorizeAsync(string, string, string[], CancellationToken)"/>.
    /// </summary>
    public bool AutomaticallyAuthorize { get; }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet API client.
    /// </summary>
    /// <param name="client">HTTP client to reuse. It is not intentionally mutated for better usage on backend.</param>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached. This is only considered after calling <see cref="AuthorizeAsync(string, string, string[], CancellationToken)"/>.</param>
    public ManiaPlanetAPI(HttpClient client, bool automaticallyAuthorize = true)
    {
        Client = client;
        AutomaticallyAuthorize = automaticallyAuthorize;
    }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet API client.
    /// </summary>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached. This is only considered after calling <see cref="AuthorizeAsync(string, string, string[], CancellationToken)"/>.</param>
    public ManiaPlanetAPI(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize) { }

    /// <summary>
    /// Authorizes with the API using the OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    /// <param name="scopes">Scopes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ManiaPlanetAPIResponseException">Status code is not 200-299.</exception>
    public virtual async Task AuthorizeAsync(string clientId, string clientSecret, string[] scopes, CancellationToken cancellationToken = default)
    {
        var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "scope", string.Join("%20", scopes) }
        };

        using var response = await Client.PostAsync($"{BaseAddress}/access_token", new FormUrlEncodedContent(values), cancellationToken);

#if DEBUG
        var stringResult = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

        await ValidateResponseAsync(response, cancellationToken);

        (_, _, accessToken) = await response.Content.ReadFromJsonAsync(ManiaPlanetAPIJsonContext.Default.AuthorizationResponse, cancellationToken) ?? throw new Exception("This shouldn't be null.");

        Payload = JwtPayloadManiaPlanetAPI.DecodeFromAccessToken(accessToken);

        // set afterwards in case the task cancels
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        
        authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    /// <summary>
    /// Authorizes with the API using the OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ManiaPlanetAPIResponseException">Status code is not 200-299.</exception>
    public async Task AuthorizeAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(clientId, clientSecret, Array.Empty<string>(), cancellationToken);
    }

    /// <summary>
    /// Validates the response and throws a specially formatted exception if it is not successful.
    /// </summary>
    /// <param name="response">HTTP response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ManiaPlanetAPIResponseException">Status code is not 200-299.</exception>
    private static async ValueTask ValidateResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ErrorResponse? error;

        try
        {
            error = await response.Content.ReadFromJsonAsync(ManiaPlanetAPIJsonContext.Default.ErrorResponse, cancellationToken);
        }
        catch (JsonException)
        {
            error = null;
        }

        throw new ManiaPlanetAPIResponseException(error, new HttpRequestException(response.ReasonPhrase, inner: null, response.StatusCode));
    }

    public virtual async Task<Player> GetPlayerAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me", ManiaPlanetAPIJsonContext.Default.Player, cancellationToken);
    }

    public virtual async Task<Title[]> GetTitlesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me/titles/created", ManiaPlanetAPIJsonContext.Default.TitleArray, cancellationToken);
    }

    public virtual async Task<DedicatedAccount[]> GetDedicatedAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me/dedicated", ManiaPlanetAPIJsonContext.Default.DedicatedAccountArray, cancellationToken);
    }

    public virtual async Task<Map[]> GetMapsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me/maps", ManiaPlanetAPIJsonContext.Default.MapArray, cancellationToken);
    }

    public virtual async Task<string> GetEmailAsync(CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync("me/email", cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public virtual async Task<Map?> GetMapByUidAsync(string uid, CancellationToken cancellationToken = default)
    {
        return await GetNullableJsonAsync($"maps/{uid}", ManiaPlanetAPIJsonContext.Default.Map, cancellationToken);
    }

    public virtual async Task<IEnumerable<string>> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        var zones = await GetJsonAsync("zones", ManiaPlanetAPIJsonContext.Default.ZoneArray, cancellationToken);

        return zones.Select(x => x.Path);
    }

    public virtual async Task<Title[]> GetTitlesAsync(
        string[]? filters = null, 
        string? orderBy = "onlinePlayers", 
        int offset = 0, 
        int length = 10, 
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("titles");
        var first = true;

        if (filters is not null)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("filters[]=");
            sb.Append(string.Join(",", filters));
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

        return await GetJsonAsync(sb.ToString(), ManiaPlanetAPIJsonContext.Default.TitleArray, cancellationToken);
    }

    public virtual async Task<Title?> GetTitleByUidAsync(string uid, CancellationToken cancellationToken = default)
    {
        return await GetNullableJsonAsync($"titles/{uid}", ManiaPlanetAPIJsonContext.Default.Title, cancellationToken);
    }

    public virtual async Task<TitleScript[]> GetTitleScriptsAsync(string uid, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"titles/{uid}/scripts", ManiaPlanetAPIJsonContext.Default.TitleScriptArray, cancellationToken);
    }

    public virtual async Task<OnlineServer[]> GetOnlineServersAsync(
        string orderBy = "playerCount",
        string[]? titleUids = null,
        string[]? environments = null,
        string? scriptName = null,
        string? search = null,
        string? zone = null,
        bool onlyPublic = false,
        bool onlyPrivate = false,
        bool onlyLobby = false,
        bool excludeLobby = true,
        int offset = 0,
        int length = 10,
        CancellationToken cancellationToken = default)
    {
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
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("titleUids[]=");
            sb.Append(string.Join(",", titleUids));
        }

        if (environments is not null)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append("environments[]=");
            sb.Append(string.Join(",", environments));
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

        return await GetJsonAsync(sb.ToString(), ManiaPlanetAPIJsonContext.Default.OnlineServerArray, cancellationToken);
    }

    protected internal async Task<HttpResponseMessage> GetResponseAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        if (AutomaticallyAuthorize && ExpirationTime.HasValue && DateTimeOffset.UtcNow >= ExpirationTime && clientId is not null && clientSecret is not null)
        {
            await AuthorizeAsync(clientId, clientSecret, cancellationToken);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/{endpoint}");

        if (accessToken is not null)
        {
            request.Headers.Authorization = authorization;
        }

        var response = await Client.SendAsync(request, cancellationToken);

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        await ValidateResponseAsync(response, cancellationToken);

        return response;
    }

    protected internal async Task<T?> GetNullableJsonAsync<T>(string endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await GetResponseAsync(endpoint, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    protected internal async Task<T> GetJsonAsync<T>(string endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        return await GetNullableJsonAsync(endpoint, jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}