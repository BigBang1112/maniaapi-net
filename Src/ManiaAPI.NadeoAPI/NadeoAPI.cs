using ManiaAPI.Base;
using ManiaAPI.Base.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ManiaAPI.NadeoAPI;

public interface INadeoAPI
{
    ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default);
}

public abstract class NadeoAPI : JsonAPI, INadeoAPI
{
    private string? accessToken;
    private string? refreshToken;

    public JwtPayloadNadeoAPI? Payload { get; private set; }

    public DateTimeOffset? RefreshAt => Payload?.RefreshAt;
    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    protected NadeoAPI(HttpClientHandler handler, string baseUrl, bool automaticallyAuthorize) : base(handler, baseUrl, automaticallyAuthorize)
    {
        SetUserAgent();
    }

    protected NadeoAPI(string baseUrl, bool automaticallyAuthorize) : base(baseUrl, automaticallyAuthorize)
    {
        SetUserAgent();
    }

    private void SetUserAgent()
    {
        Client.DefaultRequestHeaders.Add("User-Agent", $"ManiaAPI.NET ({GetType().Name}) by BigBang1112");
    }

    public async Task AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        var authenticationValue = $"{login}:{password}";
        var encodedAuthenticationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationValue));

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic");

        message.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedAuthenticationValue);
        message.Content = JsonContent.Create(new AuthorizationBody(GetType().Name));

        using var httpResponse = await Client.SendAsync(message, cancellationToken);

        await SaveTokenResponseAsync(httpResponse, cancellationToken);
    }

    internal async Task SaveTokenResponseAsync(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
    {
        await httpResponse.EnsureSuccessStatusCodeAsync(cancellationToken);

        (accessToken, refreshToken) = await httpResponse.Content.ReadFromJsonAsync<AuthorizationResponse>(JsonSerializerOptionsInObject, cancellationToken)
            ?? throw new Exception("This shouldn't be null.");

        Payload = JwtPayloadNadeoAPI.DecodeFromAccessToken(accessToken);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={accessToken}");
    }

    public async ValueTask<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (refreshToken is null || RefreshAt is null || DateTimeOffset.UtcNow < RefreshAt.Value)
        {
            return false;
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh");

        message.Headers.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={refreshToken}");

        using var httpResponse = await Client.SendAsync(message, cancellationToken);

        await SaveTokenResponseAsync(httpResponse, cancellationToken);

        return true;
    }

    protected override async Task<T> GetApiAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        if (AutomaticallyAuthorize && ExpirationTime.HasValue && DateTimeOffset.UtcNow >= ExpirationTime)
        {
            await RefreshAsync(cancellationToken);
        }

        return await base.GetApiAsync<T>(endpoint, cancellationToken);
    }
}
