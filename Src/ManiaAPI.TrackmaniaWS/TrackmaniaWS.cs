using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.TrackmaniaWS;

public interface ITrackmaniaWS : IDisposable
{
    /// <summary>
    /// HTTP client.
    /// </summary>
    HttpClient Client { get; }

    /// <summary>
    /// Gets a player by login.
    /// </summary>
    Task<Player> GetPlayerAsync(string login, CancellationToken cancellationToken = default);
}

public class TrackmaniaWS : ITrackmaniaWS
{
    public const string BaseAddress = "http://ws.trackmania.com/";

    private readonly AuthenticationHeaderValue authentication;

    public HttpClient Client { get; }

    public TrackmaniaWS(string apiUsername, string apiPassword, HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NET/2.3.1 (TrackmaniaWS; Email=petrpiv1@gmail.com; Discord=bigbang1112)");

        authentication = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUsername}:{apiPassword}")));
    }

    public TrackmaniaWS(string apiUsername, string apiPassword) : this(apiUsername, apiPassword, new HttpClient { BaseAddress = new Uri(BaseAddress) })
    {
    }

    public TrackmaniaWS(TrackmaniaWSCredentials credentials, HttpClient client) : this(credentials.ApiUsername, credentials.ApiPassword, client)
    {
    }

    public TrackmaniaWS(TrackmaniaWSCredentials credentials) : this(credentials.ApiUsername, credentials.ApiPassword)
    {
    }

    public virtual async Task<Player> GetPlayerAsync(string login, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/players/{login}/", TrackmaniaWSJsonContext.Default.Player, cancellationToken);
    }

    protected internal async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = authentication;

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
