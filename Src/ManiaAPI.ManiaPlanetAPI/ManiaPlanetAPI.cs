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
    Task<User> GetUserAsync(CancellationToken cancellationToken = default);
}

public class ManiaPlanetAPI : IManiaPlanetAPI
{
    private string? accessToken;
    private string? clientId;
    private string? clientSecret;

    public const string BaseAddress = "https://maniaplanet.com/webservices/";

    public JwtPayloadManiaPlanetAPI? Payload { get; private set; }

    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    public HttpClient Client { get; }
    public bool AutomaticallyAuthorize { get; }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached.</param>
    public ManiaPlanetAPI(HttpClient client, bool automaticallyAuthorize = true)
    {
        Client = client;
        AutomaticallyAuthorize = automaticallyAuthorize;
    }

    public ManiaPlanetAPI(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize) { }

    /// <summary>
    /// Authorizes with the official API using OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    /// <param name="scopes">Scopes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
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

        this.clientId = clientId;
        this.clientSecret = clientSecret;
    }

    public async Task AuthorizeAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(clientId, clientSecret, Array.Empty<string>(), cancellationToken);
    }

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

    public virtual async Task<User> GetUserAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me", ManiaPlanetAPIJsonContext.Default.User, cancellationToken);
    }

    public virtual async Task<Title[]> GetTitlesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me/titles/created", ManiaPlanetAPIJsonContext.Default.TitleArray, cancellationToken);
    }

    public virtual async Task<DedicatedServer[]> GetDedicatedServersAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("me/dedicated_servers", ManiaPlanetAPIJsonContext.Default.DedicatedServerArray, cancellationToken);
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

    public virtual async Task<Title[]> GetTitlesAsync(string[]? filters = null, string? orderBy = "onlinePlayers", int offset = 0, int length = 10, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder("titles");
        var first = true;

        if (filters is not null)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append($"filters[]={string.Join(",", filters)}");
        }

        if (orderBy is not null and "onlinePlayers")
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append($"orderBy={orderBy}");
        }

        if (offset != 0)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append($"offset={offset}");
        }

        if (length != 10)
        {
            sb.Append(first ? '?' : '&');
            first = false;

            sb.Append($"length={length}");
        }

        return await GetJsonAsync(sb.ToString(), ManiaPlanetAPIJsonContext.Default.TitleArray, cancellationToken);
    }

    public virtual async Task<Title?> GetTitleByUidAsync(string uid, CancellationToken cancellationToken = default)
    {
        return await GetNullableJsonAsync($"titles/{uid}", ManiaPlanetAPIJsonContext.Default.Title, cancellationToken);
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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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