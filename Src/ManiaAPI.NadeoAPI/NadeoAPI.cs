using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.NadeoAPI;

public interface INadeoAPI : IDisposable
{
    Task AuthorizeAsync(string login, string password, AuthorizationMethod method, CancellationToken cancellationToken = default);
    Task AuthorizeAsync(NadeoAPICredentials credentials, CancellationToken cancellationToken = default);
    ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default);

    HttpClient Client { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="endpoint"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NadeoAPIResponseException"></exception>
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

    private readonly SemaphoreSlim semaphore = new(1, 1);

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

        Client.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NET/2.3.1 (NadeoAPI; Email=petrpiv1@gmail.com; Discord=bigbang1112)");
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
    public virtual async Task AuthorizeAsync(string login, string password, AuthorizationMethod method, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(login);
        ArgumentException.ThrowIfNullOrEmpty(password);

        // TODO: Try optimize with Span
        var authenticationValue = $"{login}:{password}";
        var encodedAuthenticationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationValue));

        if (method == AuthorizationMethod.UbisoftAccount)
        {
            using var ubiRequest = new HttpRequestMessage(HttpMethod.Post, "https://public-ubiservices.ubi.com/v3/profiles/sessions")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Basic", encodedAuthenticationValue) },
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            ubiRequest.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");

            using var ubiResponse = await Client.SendAsync(ubiRequest, cancellationToken);

            Handler.UbisoftTicket = await ubiResponse.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.UbisoftAuthenticationTicket, cancellationToken);
        }

        var payload = new AuthorizationBody(Audience);
        var content = JsonContent.Create(payload, NadeoAPIJsonContext.Default.AuthorizationBody);
        
        var authRequest = method switch
        {
            AuthorizationMethod.UbisoftAccount => new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/ubiservices")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("ubi_v1", $"t={Handler.UbisoftTicket?.Ticket ?? throw new Exception("Ticket not available")}") },
                Content = content
            },
            AuthorizationMethod.DedicatedServer => new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Basic", encodedAuthenticationValue) },
                Content = content
            },
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
        };

        using var response = await Client.SendAsync(authRequest, cancellationToken);

        await SaveTokenResponseAsync(response, cancellationToken);
    }

    public async Task AuthorizeAsync(NadeoAPICredentials credentials, CancellationToken cancellationToken = default)
    {
        await AuthorizeAsync(credentials.Login, credentials.Password, credentials.Method, cancellationToken);
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

        var errorStr = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new NadeoAPIResponseException(errorStr, inner: ex);
        }

        throw new NadeoAPIResponseException(errorStr);
    }

    internal async Task SaveTokenResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();

#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

        var (accessToken, refreshToken) = await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.AuthorizationResponse, cancellationToken)
            ?? throw new Exception("This shouldn't be null.");

        Handler.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={accessToken ?? throw new Exception("accessToken is null")}");
        Handler.RefreshToken = refreshToken ?? throw new Exception("refreshToken is null");

        Handler.JWT = JwtPayloadNadeoAPI.DecodeFromAccessToken(accessToken);
    }

    public virtual async ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (Handler.RefreshToken is null || RefreshAt is null || DateTimeOffset.UtcNow < RefreshAt.Value || ExpirationTime is null)
        {
            return false;
        }

        // if older than 20 hours, reauthorize
        if (DateTimeOffset.UtcNow >= ExpirationTime.Value.AddHours(20))
        {
            await AuthorizeAsync(Handler.SavedCredentials ?? throw new Exception("No credentials available to reauthorize."), cancellationToken);
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={Handler.RefreshToken}") }
        };

        using var response = await Client.SendAsync(message, cancellationToken);

        await SaveTokenResponseAsync(response, cancellationToken);

        return true;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string? endpoint, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);

        if (Handler.PendingCredentials is not null)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                if (Handler.PendingCredentials is not null)
                {
                    await AuthorizeAsync(Handler.PendingCredentials, cancellationToken);
                    Handler.SavedCredentials = Handler.PendingCredentials;
                    Handler.PendingCredentials = null;
                }
            }
            finally
            {
                semaphore.Release();
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

        var response = await Client.SendAsync(request, cancellationToken);

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {response.StatusCode} {await response.Content.ReadAsStringAsync(cancellationToken)}");

        await ValidateResponseAsync(response, cancellationToken);

        return response;
    }

    protected async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, endpoint, cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    protected async Task<T?> GetNullableJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, endpoint, cancellationToken: cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }
        
        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken);
    }

    protected async Task<T> PostJsonAsync<T>(string? endpoint, JsonContent content, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, endpoint, content, cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
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
