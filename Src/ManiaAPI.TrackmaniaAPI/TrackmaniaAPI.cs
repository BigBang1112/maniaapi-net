using ManiaAPI.TrackmaniaAPI.JsonContexts;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.TrackmaniaAPI;

public interface ITrackmaniaAPI
{
    Task AuthorizeAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);
    Task AuthorizeAsync(string clientId, string clientSecret, string[] scopes, CancellationToken cancellationToken = default);
    Task<Dictionary<string, Guid>> GetAccountIdsAsync(IEnumerable<string> displayNames, CancellationToken cancellationToken = default);
    Task<Dictionary<string, Guid>> GetAccountIdsAsync(params string[] accountIds);
    Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, string>> GetDisplayNamesAsync(params Guid[] accountIds);
    Task<User> GetUserAsync(CancellationToken cancellationToken = default);
}

public class TrackmaniaAPI : ITrackmaniaAPI
{
    private string? accessToken;
    private string? clientId;
    private string? clientSecret;

    public JwtPayloadTrackmaniaAPI? Payload { get; private set; }

    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    public HttpClient Client { get; }
    public bool AutomaticallyAuthorize { get; }

    /// <summary>
    /// Creates a new instance of the Trackmania API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached.</param>
    public TrackmaniaAPI(HttpClient client, bool automaticallyAuthorize = true)
    {
        Client = client;
        Client.BaseAddress = new Uri("https://api.trackmania.com/api/");
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TrackmaniaAPI) by BigBang1112");

        AutomaticallyAuthorize = automaticallyAuthorize;
    }

    public TrackmaniaAPI(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize) { }

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

        using var response = await Client.PostAsync("access_token", new FormUrlEncodedContent(values), cancellationToken);

#if DEBUG
        var stringResult = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

        await ValidateResponseAsync(response, cancellationToken);

        (_, _, accessToken) = await response.Content.ReadFromJsonAsync(TrackmaniaAPIJsonContext.Default.AuthorizationResponse, cancellationToken) ?? throw new Exception("This shouldn't be null.");

        Payload = JwtPayloadTrackmaniaAPI.DecodeFromAccessToken(accessToken);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

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
            error = await response.Content.ReadFromJsonAsync(TrackmaniaAPIJsonContext.Default.ErrorResponse, cancellationToken);
        }
        catch (JsonException)
        {
            error = null;
        }

        throw new TrackmaniaAPIResponseException(error, new HttpRequestException(response.ReasonPhrase, inner: null, response.StatusCode));
    }

    /// <summary>
    /// Fetches the display names of the account IDs.
    /// </summary>
    /// <param name="accountIds">Account IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of nicknames.</returns>
    public virtual async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        if (!accountIds.Any())
        {
            return new Dictionary<Guid, string>();
        }

        return await GetJsonAsync($"display-names?{string.Join('&', accountIds.Select((x, i) => $"accountId[{i}]={x}"))}",
            TrackmaniaAPIJsonContext.Default.DictionaryGuidString, cancellationToken);
    }

    public async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(params Guid[] accountIds)
    {
        try
        {
            return await GetDisplayNamesAsync(accountIds, CancellationToken.None);
        }
        catch (JsonException)
        {
            return new Dictionary<Guid, string>();
        }
    }

    public virtual async Task<User> GetUserAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("user", TrackmaniaAPIJsonContext.Default.User, cancellationToken);
    }

    public virtual async Task<Dictionary<string, Guid>> GetAccountIdsAsync(IEnumerable<string> displayNames, CancellationToken cancellationToken = default)
    {
        if (!displayNames.Any())
        {
            return new Dictionary<string, Guid>();
        }

        return await GetJsonAsync($"display-names/account-ids?{string.Join('&', displayNames.Select((x, i) => $"displayName[{i}]={x}"))}",
            TrackmaniaAPIJsonContext.Default.DictionaryStringGuid, cancellationToken);
    }

    public async Task<Dictionary<string, Guid>> GetAccountIdsAsync(params string[] accountIds)
    {
        try
        {
            return await GetAccountIdsAsync(accountIds, CancellationToken.None);
        }
        catch (JsonException)
        {
            return new Dictionary<string, Guid>();
        }
    }

    protected internal async Task<T> GetJsonAsync<T>(string endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        if (AutomaticallyAuthorize && ExpirationTime.HasValue && DateTimeOffset.UtcNow >= ExpirationTime && clientId is not null && clientSecret is not null)
        {
            await AuthorizeAsync(clientId, clientSecret, cancellationToken);
        }

        using var response = await Client.GetAsync(endpoint, cancellationToken);

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        await ValidateResponseAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }
}
