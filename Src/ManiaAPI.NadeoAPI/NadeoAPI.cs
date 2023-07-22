using ManiaAPI.NadeoAPI.JsonContexts;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.NadeoAPI;

public interface INadeoAPI : IDisposable
{
    Task AuthorizeAsync(string login, string password, AuthorizationMethod method = AuthorizationMethod.DedicatedServer, CancellationToken cancellationToken = default);
    ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default);
}

public abstract class NadeoAPI : INadeoAPI
{
    private string? accessToken;
    private string? refreshToken;

    public JwtPayloadNadeoAPI? JWT { get; private set; }

    public DateTimeOffset? RefreshAt => JWT?.RefreshAt;
    public DateTimeOffset? ExpirationTime => JWT?.ExpirationTime;

    public HttpClient Client { get; }
    public bool AutomaticallyAuthorize { get; }
    public abstract string Audience { get; }

    protected NadeoAPI(HttpClient client, bool automaticallyAuthorize = true)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", $"ManiaAPI.NET ({Audience}) by BigBang1112");

        AutomaticallyAuthorize = automaticallyAuthorize;
    }

    public virtual async Task AuthorizeAsync(string login, string password, AuthorizationMethod method = AuthorizationMethod.DedicatedServer, CancellationToken cancellationToken = default)
    {
        // TODO: Try optimize with Span
        var authenticationValue = $"{login}:{password}";
        var encodedAuthenticationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationValue));

        var payload = new AuthorizationBody(Audience);

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Basic", encodedAuthenticationValue) },
            Content = new StringContent(JsonSerializer.Serialize(payload, AuthorizationBodyJsonContext.Default.AuthorizationBody), Encoding.UTF8, "application/json")
        };

        using var response = await Client.SendAsync(message, cancellationToken);

        await SaveTokenResponseAsync(response, cancellationToken);
    }

    private static async ValueTask ValidateResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadFromJsonAsync(ErrorResponseJsonContext.Default.ErrorResponse, cancellationToken);

        throw new NadeoAPIResponseException(error, new HttpRequestException(response.ReasonPhrase, inner: null, response.StatusCode));
    }

    internal async Task SaveTokenResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();

#if DEBUG
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
#endif

        (accessToken, refreshToken) = await response.Content.ReadFromJsonAsync(AuthorizationResponseJsonContext.Default.AuthorizationResponse, cancellationToken)
            ?? throw new Exception("This shouldn't be null.");

        _ = accessToken ?? throw new Exception("accessToken is null");
        _ = refreshToken ?? throw new Exception("refreshToken is null");

        JWT = JwtPayloadNadeoAPI.DecodeFromAccessToken(accessToken);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={accessToken}");
    }

    public virtual async ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (refreshToken is null || RefreshAt is null || DateTimeOffset.UtcNow < RefreshAt.Value)
        {
            return false;
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={refreshToken}") }
        };

        using var response = await Client.SendAsync(message, cancellationToken);

        await SaveTokenResponseAsync(response, cancellationToken);

        return true;
    }

    protected internal async Task<T> GetJsonAsync<T>(string endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        if (AutomaticallyAuthorize && ExpirationTime.HasValue && DateTimeOffset.UtcNow >= ExpirationTime)
        {
            await RefreshAsync(cancellationToken);
        }

        using var response = await Client.GetAsync(endpoint, cancellationToken);

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        await ValidateResponseAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

#if DEBUG
    protected async Task<string> GetAsync(string endpoint, CancellationToken cancellationToken)
    {
        using var response = await Client.GetAsync(endpoint, cancellationToken);

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        await ValidateResponseAsync(response, cancellationToken);

        return responseString;
    }
#endif

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
