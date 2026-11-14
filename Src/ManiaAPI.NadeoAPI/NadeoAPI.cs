using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.NadeoAPI;

public interface INadeoAPI : IDisposable
{
    [Obsolete("Use AuthorizeAsync(string login, string password, CancellationToken cancellationToken) instead. The Ubisoft account authentication method is no longer working (create a service account instead: https://www.trackmania.com/player/service-account)")]
    Task AuthorizeAsync(string login, string password, AuthorizationMethod method, CancellationToken cancellationToken = default);
    Task AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default);
    Task AuthorizeAsync(NadeoAPICredentials credentials, CancellationToken cancellationToken = default);
    ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default);

    HttpClient Client { get; }

    Task<HttpResponseMessage> SendAsync(HttpMethod method, string? endpoint, HttpContent? content = null, CancellationToken cancellationToken = default);
}

public abstract class NadeoAPI : INadeoAPI
{
    public NadeoAPIHandler Handler { get; }

    public HttpClient Client { get; }
    public bool AutomaticallyAuthorize { get; }

    public abstract string BaseAddress { get; }
    public abstract string Audience { get; }

    public DateTimeOffset? RefreshAt => Handler.JWT?.RefreshAt;
    public DateTimeOffset? ExpirationTime => Handler.JWT?.ExpirationTime;

    /// <summary>
    /// Creates a client that sends requests through <paramref name="client"/>, authorized with the tokens kept in <paramref name="handler"/>.
    /// </summary>
    /// <param name="client">HTTP client to send requests with. The library's User-Agent is added to it.</param>
    /// <param name="handler">Holds the tokens. Clients that share it share the authorization.</param>
    /// <param name="automaticallyAuthorize">Whether requests refresh the token once its refresh time has passed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> or <paramref name="handler"/> is null.</exception>
    protected NadeoAPI(HttpClient client, NadeoAPIHandler handler, bool automaticallyAuthorize = true)
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
            headers.UserAgent.Add(new ProductInfoHeaderValue("(NadeoAPI; Email=petrpiv1@gmail.com; Discord=bigbang1112)"));
        }
    }

    /// <summary>
    /// Signs in with a service account. <paramref name="method"/> is ignored, as Ubisoft account authentication no longer works.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="login"/> or <paramref name="password"/> is null or empty.</exception>
    /// <exception cref="NadeoAPIResponseException">The request failed or the response is an error.</exception>
    [Obsolete("Use AuthorizeAsync(string login, string password, CancellationToken cancellationToken) instead. The Ubisoft account authentication method is no longer working (create a service account instead: https://www.trackmania.com/player/service-account)")]
    public virtual async Task AuthorizeAsync(string login, string password, AuthorizationMethod method, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(login, password, cancellationToken);
    }

    /// <summary>
    /// Signs in with a service account and stores the tokens in <see cref="Handler"/>.
    /// The credentials are also kept for signing in again later when <see cref="NadeoAPIHandler.SaveCredentials"/> is true.
    /// </summary>
    /// <param name="login">Login of the service account.</param>
    /// <param name="password">Password of the service account.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException"><paramref name="login"/> or <paramref name="password"/> is null or empty.</exception>
    /// <exception cref="NadeoAPIResponseException">The request failed or the response is an error.</exception>
    /// <exception cref="JsonException">The response has no usable tokens.</exception>
    public virtual async Task AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(login);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var authenticationValue = $"{login}:{password}";
        var encodedAuthenticationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationValue));

        var payload = new AuthorizationBody(Audience);
        var content = JsonContent.Create(payload, NadeoAPIJsonContext.Default.AuthorizationBody);

        var authRequest = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Basic", encodedAuthenticationValue) },
            Content = content
        };

        await RequestTokenAsync(authRequest, cancellationToken);

        // Kept so RefreshAsync can reauthorize once the refresh token has expired too
        if (Handler.SaveCredentials)
        {
            Handler.SavedCredentials = new NadeoAPICredentials(login, password);
        }
    }

    public async Task AuthorizeAsync(NadeoAPICredentials credentials, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(credentials.Login, credentials.Password, cancellationToken);
    }

    /// <summary>
    /// Throws for an unsuccessful response, with the error from its body when it has one.
    /// </summary>
    /// <exception cref="NadeoAPIResponseException">The response is an error.</exception>
    private static async ValueTask ValidateResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ErrorResponse? error;

        try
        {
            error = await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.ErrorResponse, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // Not JSON, or a charset that can't be decoded, so only the status describes the error
            error = null;
        }

        throw new NadeoAPIResponseException(error, response.StatusCode, response.ReasonPhrase);
    }

    /// <summary>
    /// Sends a request to the authentication endpoint and stores the tokens from the response.
    /// </summary>
    /// <exception cref="NadeoAPIResponseException">The request failed or the response is an error.</exception>
    /// <exception cref="JsonException">The response has no usable tokens.</exception>
    private async Task RequestTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await Client.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new NadeoAPIResponseException(ex.Message, ex);
        }

        using (response)
        {
            await ValidateResponseAsync(response, cancellationToken);

#if DEBUG
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

            var (accessToken, refreshToken) = await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.AuthorizationResponse, cancellationToken)
                ?? throw new JsonException("The authentication response body is null.");

            if (accessToken is null || refreshToken is null)
            {
                throw new JsonException("The authentication response is missing the access token or the refresh token.");
            }

            Handler.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={accessToken}");
            Handler.RefreshToken = refreshToken;

            Handler.JWT = JwtPayloadNadeoAPI.DecodeFromAccessToken(accessToken);

            // Already authorized, so the next request doesn't sign in with them again
            Handler.PendingCredentials = null;
        }
    }

    public virtual async ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRefreshDue())
        {
            return false;
        }

        await Handler.AuthorizationLock.WaitAsync(cancellationToken);

        try
        {
            // Another client sharing the handler may have refreshed while this one waited
            if (!IsRefreshDue())
            {
                return false;
            }

            // If expired for over 20 hours, the refresh token is likely expired too, so reauthorize when credentials were saved.
            // Without saved credentials, try the refresh token anyway.
            if (Handler.SaveCredentials && Handler.SavedCredentials is not null && DateTimeOffset.UtcNow >= ExpirationTime!.Value.AddHours(20))
            {
                await AuthorizeAsync(Handler.SavedCredentials, cancellationToken);
                return true;
            }

            using var message = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={Handler.RefreshToken}") }
            };

            await RequestTokenAsync(message, cancellationToken);

            return true;
        }
        finally
        {
            Handler.AuthorizationLock.Release();
        }
    }

    /// <summary>
    /// Signs in with the handler's pending credentials, unless Nadeo already rejected them.
    /// </summary>
    /// <exception cref="NadeoAPIResponseException">Signing in failed now or the credentials were rejected before.</exception>
    private async Task AuthorizePendingAsync(CancellationToken cancellationToken)
    {
        await Handler.AuthorizationLock.WaitAsync(cancellationToken);

        try
        {
            // Another client sharing the handler may have authorized while this one waited
            if (Handler.PendingCredentials is not { } credentials)
            {
                return;
            }

            if (Handler.CredentialsRejection is { } rejection && rejection.Credentials == credentials)
            {
                // Only rejections with a 401 or 403 status are stored. The original is kept for its stack trace and correlation.
                throw new NadeoAPIResponseException(rejection.Error.Response, rejection.Error.StatusCode!.Value, rejection.Error.ReasonPhrase, rejection.Error);
            }

            try
            {
                await AuthorizeAsync(credentials, cancellationToken);
            }
            catch (NadeoAPIResponseException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                // Wrong credentials stay wrong, so they aren't sent again until they're replaced
                Handler.CredentialsRejection = (credentials, ex);
                throw;
            }
        }
        finally
        {
            Handler.AuthorizationLock.Release();
        }
    }

    private bool IsRefreshDue()
    {
        return Handler.RefreshToken is not null
            && RefreshAt.HasValue
            && ExpirationTime.HasValue
            && DateTimeOffset.UtcNow >= RefreshAt.Value;
    }

    /// <summary>
    /// Sends an authorized request to an endpoint of this service, signing in with pending credentials or refreshing the token first when needed.
    /// </summary>
    /// <param name="method">HTTP method of the request.</param>
    /// <param name="endpoint">Path relative to <see cref="BaseAddress"/>, including any query.</param>
    /// <param name="content">Body of the request, disposed with it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The successful response, which the caller disposes.</returns>
    /// <exception cref="NadeoAPIResponseException">The request failed or the response is an error.</exception>
    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string? endpoint, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (Handler.PendingCredentials is not null)
        {
            await AuthorizePendingAsync(cancellationToken);
        }

        // Refreshed from the refresh time rather than the expiration, so a request isn't sent with a token about to expire
        if (AutomaticallyAuthorize && RefreshAt.HasValue && DateTimeOffset.UtcNow >= RefreshAt)
        {
            await RefreshAsync(cancellationToken);
        }

        using var request = new HttpRequestMessage(method, $"{BaseAddress}/{endpoint}");
        request.Headers.Authorization = Handler.Authorization;

        if (content is not null)
        {
            request.Content = content;
        }

        HttpResponseMessage response;

        try
        {
            response = await Client.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new NadeoAPIResponseException(ex.Message, ex);
        }

        try
        {
            await ValidateResponseAsync(response, cancellationToken);

            // After validation, so an error body that can't be read as a string still throws the response exception in Debug builds
            Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {response.StatusCode} {await response.Content.ReadAsStringAsync(cancellationToken)}");

            return response;
        }
        catch
        {
            // The caller only disposes a response it gets back
            response.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Sends a GET request and reads the JSON response.
    /// </summary>
    /// <exception cref="NadeoAPIResponseException">The request failed or the response is an error.</exception>
    /// <exception cref="JsonException">The response body is null or isn't valid JSON for <typeparamref name="T"/>.</exception>
    protected async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, endpoint, cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new JsonException("The response body is null.");
    }

    /// <summary>
    /// Sends a GET request and reads the JSON response, or returns the default value when the response has no content (HTTP 204).
    /// </summary>
    /// <exception cref="NadeoAPIResponseException">The request failed or the response is an error.</exception>
    /// <exception cref="JsonException">The response body isn't valid JSON for <typeparamref name="T"/>.</exception>
    protected async Task<T?> GetNullableJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, endpoint, cancellationToken: cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }
        
        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// Sends a POST request with a JSON body and reads the JSON response.
    /// </summary>
    /// <exception cref="NadeoAPIResponseException">The request failed or the response is an error.</exception>
    /// <exception cref="JsonException">The response body is null or isn't valid JSON for <typeparamref name="T"/>.</exception>
    protected async Task<T> PostJsonAsync<T>(string? endpoint, JsonContent content, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, endpoint, content, cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new JsonException("The response body is null.");
    }

#if DEBUG
    protected async Task<string> GetAsync(string? endpoint, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/{endpoint}");
        request.Headers.Authorization = Handler.Authorization;

        using var response = await Client.SendAsync(request, cancellationToken);

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        await ValidateResponseAsync(response, cancellationToken);

        return responseString;
    }
#endif

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
