using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.TrackmaniaAPI;

public interface ITrackmaniaAPI : IDisposable
{
    Task AuthorizeAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default);
    Task AuthorizeAsync(string clientId, string clientSecret, IEnumerable<string> scopes, CancellationToken cancellationToken = default);
    Task AuthorizeAsync(TrackmaniaAPICredentials credentials, CancellationToken cancellationToken = default);
    Task<ImmutableDictionary<string, Guid>> GetAccountIdsAsync(IEnumerable<string> displayNames, CancellationToken cancellationToken = default);
    Task<ImmutableDictionary<string, Guid>> GetAccountIdsAsync(params string[] displayNames);
    Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(params Guid[] accountIds);
    Task<User> GetUserAsync(CancellationToken cancellationToken = default);
    Task<ImmutableList<MapRecord>> GetUserMapRecordsAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the clubs the player is an admin of. Requires the <c>clubs</c> scope and a token associated with a player (not the client credentials flow).
    /// </summary>
    Task<ImmutableList<AdminClub>> GetUserAdminClubsAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the player's favorite maps. Requires the <c>read_favorite</c> scope and a token associated with a player (not the client credentials flow).
    /// </summary>
    Task<ImmutableList<FavoriteMap>> GetUserFavoriteMapsAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// Adds a map to the player's favorite maps. Requires the <c>write_favorite</c> scope and a token associated with a player (not the client credentials flow).
    /// </summary>
    Task AddUserFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default);
    /// <summary>
    /// Removes a map from the player's favorite maps. Requires the <c>write_favorite</c> scope and a token associated with a player (not the client credentials flow).
    /// </summary>
    Task RemoveUserFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default);
}

public class TrackmaniaAPI : ITrackmaniaAPI
{
    public const string BaseAddress = "https://api.trackmania.com/api";

    public TrackmaniaAPIHandler Handler { get; }

    public DateTimeOffset? ExpirationTime => Handler.Payload?.ExpirationTime;

    public HttpClient Client { get; }
    public bool AutomaticallyAuthorize { get; }

    /// <summary>
    /// Creates a new instance of the Trackmania API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="handler">Handler to use for the Trackmania API.</param>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> or <paramref name="handler"/> is null.</exception>
    public TrackmaniaAPI(HttpClient client, TrackmaniaAPIHandler handler, bool automaticallyAuthorize = true)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        AutomaticallyAuthorize = automaticallyAuthorize;

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(TrackmaniaAPI; Email=petrpiv1@gmail.com; Discord=bigbang1112)"));
        }
    }

    public TrackmaniaAPI(bool automaticallyAuthorize = true) : this(new HttpClient(), new TrackmaniaAPIHandler(), automaticallyAuthorize) { }

    /// <summary>
    /// Authorizes with the official API using OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    /// <param name="scopes">Scopes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException"><paramref name="clientId"/> or <paramref name="clientSecret"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="scopes"/> is null.</exception>
    public virtual async Task AuthorizeAsync(string clientId, string clientSecret, IEnumerable<string> scopes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientId);
        ArgumentException.ThrowIfNullOrEmpty(clientSecret);
        ArgumentNullException.ThrowIfNull(scopes);

        var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "scope", string.Join(' ', scopes) }
        };

        using var content = new FormUrlEncodedContent(values);

        HttpResponseMessage response;

        try
        {
            response = await Client.PostAsync($"{BaseAddress}/access_token", content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new TrackmaniaAPIResponseException(ex.Message, ex);
        }

        using var _ = response;

#if DEBUG
        var stringResult = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

        await ValidateResponseAsync(response, cancellationToken);

        var (_, _, accessToken) = await response.Content.ReadFromJsonAsync(TrackmaniaAPIJsonContext.Default.AuthorizationResponse, cancellationToken) ?? throw new JsonException("The response body is null.");

        Handler.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        Handler.Payload = JwtPayloadTrackmaniaAPI.DecodeFromAccessToken(accessToken);

        Handler.Credentials = new TrackmaniaAPICredentials(
            clientId, 
            clientSecret, 
            scopes is ImmutableArray<string> immutableArray ? immutableArray : scopes.ToImmutableArray());
    }

    public async Task AuthorizeAsync(string clientId, string clientSecret, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(clientId, clientSecret, [], cancellationToken);
    }

    public async Task AuthorizeAsync(TrackmaniaAPICredentials credentials, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(credentials.ClientId, credentials.ClientSecret, credentials.Scopes, cancellationToken);
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
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // Not JSON, or a charset that can't be decoded, so only the status describes the error
            error = null;
        }

        throw new TrackmaniaAPIResponseException(error, response.StatusCode, response.ReasonPhrase);
    }

    /// <summary>
    /// Fetches the display names of the account IDs.
    /// </summary>
    /// <param name="accountIds">Account IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of nicknames.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="accountIds"/> is null.</exception>
    public virtual async Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        if (!accountIds.Any())
        {
            return ImmutableDictionary<Guid, string>.Empty;
        }

        return await GetDictionaryJsonAsync($"display-names?{string.Join('&', accountIds.Select((x, i) => $"accountId[{i}]={x}"))}",
            TrackmaniaAPIJsonContext.Default.ImmutableDictionaryGuidString, cancellationToken);
    }

    public async Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(params Guid[] accountIds)
    {
        return await GetDisplayNamesAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<User> GetUserAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("user", TrackmaniaAPIJsonContext.Default.User, cancellationToken);
    }

    public virtual async Task<ImmutableDictionary<string, Guid>> GetAccountIdsAsync(IEnumerable<string> displayNames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(displayNames);

        if (!displayNames.Any())
        {
            return ImmutableDictionary<string, Guid>.Empty;
        }

        return await GetDictionaryJsonAsync($"display-names/account-ids?{string.Join('&', displayNames.Select((x, i) => $"displayName[{i}]={Uri.EscapeDataString(x)}"))}",
            TrackmaniaAPIJsonContext.Default.ImmutableDictionaryStringGuid, cancellationToken);
    }

    public async Task<ImmutableDictionary<string, Guid>> GetAccountIdsAsync(params string[] accountIds)
    {
        return await GetAccountIdsAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<ImmutableList<MapRecord>> GetUserMapRecordsAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapIds);

        if (!mapIds.Any())
        {
            return [];
        }

        return await GetJsonAsync($"user/map-records?{string.Join('&', mapIds.Select((x, i) => $"mapId[{i}]={x}"))}",
            TrackmaniaAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<ImmutableList<AdminClub>> GetUserAdminClubsAsync(CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync("user/clubs/admin", TrackmaniaAPIJsonContext.Default.AdminClubCollection, cancellationToken)).Clubs;
    }

    public virtual async Task<ImmutableList<FavoriteMap>> GetUserFavoriteMapsAsync(int length = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"user/maps/favorite?length={length}&offset={offset}", TrackmaniaAPIJsonContext.Default.FavoriteMapCollection, cancellationToken)).List;
    }

    public virtual async Task AddUserFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        var content = JsonContent.Create(new FavoriteMapRequest(mapUid), TrackmaniaAPIJsonContext.Default.FavoriteMapRequest);
        using var response = await SendAsync(HttpMethod.Post, "user/maps/favorite/add", content, cancellationToken);
    }

    public virtual async Task RemoveUserFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        var content = JsonContent.Create(new FavoriteMapRequest(mapUid), TrackmaniaAPIJsonContext.Default.FavoriteMapRequest);
        using var response = await SendAsync(HttpMethod.Post, "user/maps/favorite/remove", content, cancellationToken);
    }

    /// <summary>
    /// Like <see cref="GetJsonAsync{T}"/>, but for endpoints that answer with an empty array instead of an empty object when nothing matches.
    /// </summary>
    private async Task<ImmutableDictionary<TKey, TValue>> GetDictionaryJsonAsync<TKey, TValue>(
        string endpoint,
        JsonTypeInfo<ImmutableDictionary<TKey, TValue>> jsonTypeInfo,
        CancellationToken cancellationToken) where TKey : notnull
    {
        using var response = await SendAsync(HttpMethod.Get, endpoint, content: null, cancellationToken);

        byte[] json;

        try
        {
            json = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new TrackmaniaAPIResponseException(ex.Message, ex);
        }

        if (IsEmptyArray(json))
        {
            return ImmutableDictionary<TKey, TValue>.Empty;
        }

        return JsonSerializer.Deserialize(json, jsonTypeInfo) ?? throw new JsonException("The response body is null.");
    }

    private static bool IsEmptyArray(ReadOnlySpan<byte> json)
    {
        var reader = new Utf8JsonReader(json);

        return reader.Read() && reader.TokenType == JsonTokenType.StartArray
            && reader.Read() && reader.TokenType == JsonTokenType.EndArray;
    }

    protected internal async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, endpoint, content: null, cancellationToken);

        try
        {
            return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new JsonException("The response body is null.");
        }
        catch (HttpRequestException ex)
        {
            throw new TrackmaniaAPIResponseException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Sends an authorized request and validates the response. The caller is responsible for disposing the returned response.
    /// </summary>
    protected internal async Task<HttpResponseMessage> SendAsync(HttpMethod method, string? endpoint, HttpContent? content, CancellationToken cancellationToken = default)
    {
        if (AutomaticallyAuthorize && IsAuthorizationDue(out _))
        {
            await Handler.AuthorizationLock.WaitAsync(cancellationToken);

            try
            {
                // Another client sharing the handler may have authorized while this one waited
                if (IsAuthorizationDue(out var credentials))
                {
                    await AuthorizeAsync(credentials, cancellationToken);
                }
            }
            finally
            {
                Handler.AuthorizationLock.Release();
            }
        }

        using var request = new HttpRequestMessage(method, $"{BaseAddress}/{endpoint}") { Content = content };
        request.Headers.Authorization = Handler.Authorization;

        HttpResponseMessage response;

        try
        {
            response = await Client.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new TrackmaniaAPIResponseException(ex.Message, ex);
        }

        try
        {
            await ValidateResponseAsync(response, cancellationToken);

            // After validation, so an error body that can't be read as a string still throws the response exception in Debug builds
            Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private bool IsAuthorizationDue([NotNullWhen(true)] out TrackmaniaAPICredentials? credentials)
    {
        credentials = Handler.Credentials;
        return credentials is not null && (ExpirationTime is null || DateTimeOffset.UtcNow >= ExpirationTime);
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
