﻿using ManiaAPI.TrackmaniaAPI.JsonContexts;
using System.Collections.Immutable;
using System.Diagnostics;
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
    Task<ImmutableDictionary<string, Guid>> GetAccountIdsAsync(params string[] accountIds);
    Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(params Guid[] accountIds);
    Task<User> GetUserAsync(CancellationToken cancellationToken = default);
    Task<ImmutableArray<MapRecord>> GetUserMapRecordsAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
}

public class TrackmaniaAPI : ITrackmaniaAPI
{
    public const string BaseAddress = "https://api.trackmania.com/api";

    public TrackmaniaAPIHandler Handler { get; }

    public DateTimeOffset? ExpirationTime => Handler.Payload?.ExpirationTime;

    public HttpClient Client { get; }
    public bool AutomaticallyAuthorize { get; }

    private readonly SemaphoreSlim semaphore = new(1, 1);

    /// <summary>
    /// Creates a new instance of the Trackmania API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="handler">Handler to use for the Trackmania API.</param>
    /// <param name="automaticallyAuthorize">If calling an endpoint should automatically try to authorize the OAuth2 client when the <see cref="ExpirationTime"/> is reached.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public TrackmaniaAPI(HttpClient client, TrackmaniaAPIHandler handler, bool automaticallyAuthorize = true)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        AutomaticallyAuthorize = automaticallyAuthorize;
    }

    public TrackmaniaAPI(bool automaticallyAuthorize = true) : this(new HttpClient(), new TrackmaniaAPIHandler(), automaticallyAuthorize) { }

    /// <summary>
    /// Authorizes with the official API using OAuth2 client credentials.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="clientSecret">Client secret.</param>
    /// <param name="scopes">Scopes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
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
            { "scope", string.Join("%20", scopes) }
        };

        using var response = await Client.PostAsync($"{BaseAddress}/access_token", new FormUrlEncodedContent(values), cancellationToken);

#if DEBUG
        var stringResult = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

        await ValidateResponseAsync(response, cancellationToken);

        var (_, _, accessToken) = await response.Content.ReadFromJsonAsync(TrackmaniaAPIJsonContext.Default.AuthorizationResponse, cancellationToken) ?? throw new Exception("This shouldn't be null.");

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
    /// <exception cref="ArgumentNullException"></exception>
    public virtual async Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        if (!accountIds.Any())
        {
            return ImmutableDictionary<Guid, string>.Empty;
        }

        return await GetJsonAsync($"display-names?{string.Join('&', accountIds.Select((x, i) => $"accountId[{i}]={x}"))}",
            TrackmaniaAPIJsonContext.Default.ImmutableDictionaryGuidString, cancellationToken);
    }

    public async Task<ImmutableDictionary<Guid, string>> GetDisplayNamesAsync(params Guid[] accountIds)
    {
        try
        {
            return await GetDisplayNamesAsync(accountIds, CancellationToken.None);
        }
        catch (JsonException)
        {
            return ImmutableDictionary<Guid, string>.Empty;
        }
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

        return await GetJsonAsync($"display-names/account-ids?{string.Join('&', displayNames.Select((x, i) => $"displayName[{i}]={x}"))}",
            TrackmaniaAPIJsonContext.Default.ImmutableDictionaryStringGuid, cancellationToken);
    }

    public async Task<ImmutableDictionary<string, Guid>> GetAccountIdsAsync(params string[] accountIds)
    {
        try
        {
            return await GetAccountIdsAsync(accountIds, CancellationToken.None);
        }
        catch (JsonException)
        {
            return ImmutableDictionary<string, Guid>.Empty;
        }
    }

    public virtual async Task<ImmutableArray<MapRecord>> GetUserMapRecordsAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapIds);

        if (!mapIds.Any())
        {
            return [];
        }

        return await GetJsonAsync($"user/map-records?{string.Join('&', mapIds.Select((x, i) => $"mapId[{i}]={x}"))}",
            TrackmaniaAPIJsonContext.Default.ImmutableArrayMapRecord, cancellationToken);
    }

    protected internal async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (AutomaticallyAuthorize && Handler.Credentials is not null
                && (ExpirationTime is null || DateTimeOffset.UtcNow >= ExpirationTime))
            {
                await AuthorizeAsync(Handler.Credentials, cancellationToken);
            }
        }
        finally
        {
            semaphore.Release();
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/{endpoint}");
        request.Headers.Authorization = Handler.Authorization;

        using var response = await Client.SendAsync(request, cancellationToken);

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        await ValidateResponseAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
