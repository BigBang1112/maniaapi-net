using ManiaAPI.Base.Converters;
using ManiaAPI.NadeoAPI.Extensions;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ManiaAPI.NadeoAPI;

public abstract class NadeoAPI
{
    private string? accessToken;
    private string? refreshToken;

    public HttpClient Client { get; }
    public JwtPayload? Payload { get; private set; }
    
    public DateTimeOffset? RefreshAt => Payload?.RefreshAt;
    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    internal static JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    static NadeoAPI()
    {
        JsonSerializerOptions.Converters.Add(new TimeInt32Converter());
    }

    protected NadeoAPI(string baseUrl)
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (NadeoAPI) by BigBang1112");
    }

    public async Task AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        var authenticationValue = $"{login}:{password}";
        var encodedAuthenticationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationValue));

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic");

        message.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedAuthenticationValue);
        message.Content = JsonContent.Create(new AuthorizationBody(GetType().Name));

        using var httpResponse = await Client.SendAsync(message, cancellationToken);
        
        await httpResponse.EnsureSuccessStatusCodeAsync();

        (accessToken, refreshToken) = await httpResponse.Content.ReadFromJsonAsync<AuthorizationResponse>(JsonSerializerOptions, cancellationToken)
            ?? throw new Exception("This shouldn't be null.");

        Payload = JwtPayload.DecodeFromAccessToken(accessToken);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={accessToken}");
    }

    protected async Task<T> GetApiAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync(requestUri, cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();

#if DEBUG
        if (typeof(T) == typeof(string))
        {
            return (T)(object)await response.Content.ReadAsStringAsync(cancellationToken);
        }
#endif

        return await response.Content.ReadFromJsonAsync<T>(JsonSerializerOptions, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }
}
