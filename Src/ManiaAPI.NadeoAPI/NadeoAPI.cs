using ManiaAPI.Base;
using ManiaAPI.Base.Converters;
using ManiaAPI.Base.Extensions;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ManiaAPI.NadeoAPI;

public abstract class NadeoAPI : JsonApiBase
{
    private string? accessToken;
    private string? refreshToken;

    public JwtPayloadNadeoAPI? Payload { get; private set; }
    
    public DateTimeOffset? RefreshAt => Payload?.RefreshAt;
    public DateTimeOffset? ExpirationTime => Payload?.ExpirationTime;

    protected NadeoAPI(string baseUrl) : base(baseUrl)
    {
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

        Payload = JwtPayloadNadeoAPI.DecodeFromAccessToken(accessToken);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("nadeo_v1", $"t={accessToken}");
    }
}
