using ManiaAPI.Base;
using ManiaAPI.Base.Exceptions;
using ManiaAPI.Base.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ManiaAPI.TrackmaniaAPI;

public class TrackmaniaAPI : JsonAPI, ITrackmaniaAPI
{
    private string? accessToken;
    private string? clientId;
    private string? clientSecret;

    public JwtPayloadTrackmaniaAPI? Payload { get; private set; }

    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    /// <summary>
    /// Creates a new instance of the Trackmania API client.
    /// </summary>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached.</param>
    public TrackmaniaAPI(bool automaticallyAuthorize = true) : base("https://api.trackmania.com/api/", automaticallyAuthorize)
    {
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TrackmaniaAPI) by BigBang1112");
    }

    /// <summary>
    /// Authorizes with the official API using OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ApiRequestException">API responded with an error message.</exception>
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
    /// <exception cref="ApiRequestException">API responded with an error message.</exception>
    public async Task<Dictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<Dictionary<Guid, string>>($"display-names?{string.Join('&', accountIds.Select((x, i) => $"accountId[{i}]={x}"))}", cancellationToken);
    }

    public async Task<User> GetUserAsync(CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<User>("user", cancellationToken);
    }

    /// <summary>
    /// Does a general API call that expects a JSON result. If access token was requested but is expired, new one is requested.
    /// </summary>
    /// <typeparam name="T">Type of the response object.</typeparam>
    /// <param name="endpoint">Endpoint to call.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response object asynchronously.</returns>
    /// <exception cref="ApiRequestException">API responded with an error message.</exception>
    protected override async Task<T> GetApiAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        if (AutomaticallyAuthorize && ExpirationTime.HasValue && DateTimeOffset.UtcNow >= ExpirationTime && clientId is not null && clientSecret is not null)
        {
            await AuthorizeAsync(clientId, clientSecret, cancellationToken);
        }

        return await base.GetApiAsync<T>(endpoint, cancellationToken);
    }
}
