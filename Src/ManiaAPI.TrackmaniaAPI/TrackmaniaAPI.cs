using ManiaAPI.Base.Converters;
using ManiaAPI.TrackmaniaAPI.Exceptions;
using ManiaAPI.TrackmaniaAPI.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManiaAPI.TrackmaniaAPI;

public class TrackmaniaAPI
{
    private string? accessToken;
    private string? clientId;
    private string? clientSecret;

    private readonly bool automaticallyAuthorize;

    public HttpClient Client { get; }
    public JwtPayloadTrackmaniaAPI? Payload { get; private set; }

    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    internal static JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    static TrackmaniaAPI()
    {
        JsonSerializerOptions.Converters.Add(new TimeInt32Converter());
    }

    /// <summary>
    /// Creates a new instance of the Trackmania API client.
    /// </summary>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached.</param>
    public TrackmaniaAPI(bool automaticallyAuthorize = true)
    {
        this.automaticallyAuthorize = automaticallyAuthorize;

        Client = new HttpClient
        {
            BaseAddress = new Uri("https://api.trackmania.com/api/")
        };

        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TrackmaniaAPI) by BigBang1112");
    }

    /// <summary>
    /// Authorizes with the official API using OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="TrackmaniaApiRequestException">API responded with an error message.</exception>
    public async Task AuthorizeAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
    {
        var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "scope", "" }
        };

        using var response = await Client.PostAsync("access_token", new FormUrlEncodedContent(values), cancellationToken);

#if DEBUG
        var stringResult = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

        await response.EnsureSuccessStatusCodeAsync(cancellationToken);

        (_, _, accessToken) = await response.Content.ReadFromJsonAsync<AuthorizationResponse>(JsonSerializerOptions, cancellationToken) ?? throw new Exception("This shouldn't be null.");

        Payload = JwtPayloadTrackmaniaAPI.DecodeFromAccessToken(accessToken);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        this.clientId = clientId;
        this.clientSecret = clientSecret;
    }

    /// <summary>
    /// Fetches the display names of the account IDs.
    /// </summary>
    /// <param name="accountIds">Account IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of nicknames.</returns>
    /// <exception cref="TrackmaniaApiRequestException">API responded with an error message.</exception>
    public async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<Dictionary<Guid, string>>($"display-names?{string.Join('&', accountIds.Select((x, i) => $"accountId[{i}]={x}"))}", cancellationToken);
    }

    /// <summary>
    /// Does a general API call that expects a JSON result. If access token was requested but is expired, new one is requested.
    /// </summary>
    /// <typeparam name="T">Type of the response object.</typeparam>
    /// <param name="endpoint">Endpoint to call.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response object asynchronously.</returns>
    /// <exception cref="TrackmaniaApiRequestException">API responded with an error message.</exception>
    private async Task<T> GetApiAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        if (automaticallyAuthorize && ExpirationTime.HasValue && DateTimeOffset.UtcNow >= ExpirationTime && clientId is not null && clientSecret is not null)
        {
            await AuthorizeAsync(clientId, clientSecret, cancellationToken);
        }

        using var response = await Client.GetAsync(endpoint, cancellationToken);

        await response.EnsureSuccessStatusCodeAsync(cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(JsonSerializerOptions, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }
}
