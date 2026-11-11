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
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="handler"></param>
    /// <param name="automaticallyAuthorize"></param>
    /// <exception cref="ArgumentNullException"></exception>
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
    /// 
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <param name="method"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [Obsolete("Use AuthorizeAsync(string login, string password, CancellationToken cancellationToken) instead. The Ubisoft account authentication method is no longer working (create a service account instead: https://www.trackmania.com/player/service-account)")]
    public virtual async Task AuthorizeAsync(string login, string password, AuthorizationMethod method, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(login, password, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
    /// 
    /// </summary>
    /// <param name="response"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NadeoAPIResponseException"></exception>
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
        catch (JsonException)
        {
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

    private bool IsRefreshDue()
    {
        return Handler.RefreshToken is not null
            && RefreshAt.HasValue
            && ExpirationTime.HasValue
            && DateTimeOffset.UtcNow >= RefreshAt.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="endpoint"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string? endpoint, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (Handler.PendingCredentials is not null)
        {
            await Handler.AuthorizationLock.WaitAsync(cancellationToken);
            try
            {
                // Another client sharing the handler may have authorized while this one waited
                if (Handler.PendingCredentials is not null)
                {
                    await AuthorizeAsync(Handler.PendingCredentials, cancellationToken);

                    Handler.PendingCredentials = null;
                }
            }
            finally
            {
                Handler.AuthorizationLock.Release();
            }
        }

        if (AutomaticallyAuthorize && ExpirationTime.HasValue && DateTimeOffset.UtcNow >= ExpirationTime)
        {
            await RefreshAsync(cancellationToken);
        }

        using var request = new HttpRequestMessage(method, $"{BaseAddress}/{endpoint}");
        request.Headers.Authorization = Handler.Authorization;

        if (content is not null)
        {
            request.Content = content;
        }

        try
        {
            var response = await Client.SendAsync(request, cancellationToken);

            Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {response.StatusCode} {await response.Content.ReadAsStringAsync(cancellationToken)}");

            await ValidateResponseAsync(response, cancellationToken);

            return response;
        }
        catch (HttpRequestException ex)
        {
            throw new NadeoAPIResponseException(ex.Message, ex.InnerException);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NadeoAPIResponseException"></exception>
    protected async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, endpoint, cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new JsonException("The response body is null.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NadeoAPIResponseException"></exception>
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
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="endpoint"></param>
    /// <param name="content"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NadeoAPIResponseException"></exception>
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
