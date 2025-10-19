using System.Net.Http.Json;

namespace ManiaAPI.ManiaPlanetAPI;

public interface IManiaPlanetIngameAPI
{
    HttpClient Client { get; }

    Task<ManiaPlanetIngameAuthResponse> AuthenticateAsync(string login, string token, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DownloadTitleAsync(HttpMethod method, string titleId, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DownloadTitleAsync(string titleId, CancellationToken cancellationToken = default);
    Task<IngameTitle?> GetTitleByUidAsync(string uid, CancellationToken cancellationToken = default);
}

public class ManiaPlanetIngameAPI : IManiaPlanetIngameAPI
{
    public const string BaseAddress = "https://prod.live.maniaplanet.com/ingame/";

    public HttpClient Client { get; }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet ingame API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public ManiaPlanetIngameAPI(HttpClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        Client.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NET/2.5.1 (ManiaPlanetIngameAPI; Email=petrpiv1@gmail.com; Discord=bigbang1112)");
    }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet ingame API client.
    /// </summary>
    public ManiaPlanetIngameAPI() : this(new HttpClient { BaseAddress = new Uri(BaseAddress) }) { }

    public async Task<ManiaPlanetIngameAuthResponse> AuthenticateAsync(string login, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(login);
        ArgumentException.ThrowIfNullOrEmpty(token);

        using var request = new HttpRequestMessage(HttpMethod.Get, "auth");
        request.Headers.Add("Maniaplanet-Auth", $"Login=\"{login}\", Token=\"{token}\"");
        request.Headers.Accept.Add(new("application/json"));

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ManiaPlanetIngameAPIJsonContext.Default.ManiaPlanetIngameAuthResponse, cancellationToken) ?? throw new Exception("This should never happen");
    }

    public async Task<HttpResponseMessage> DownloadTitleAsync(string titleId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(titleId);

        return await DownloadTitleAsync(HttpMethod.Get, titleId, cancellationToken);
    }

    public async Task<HttpResponseMessage> DownloadTitleAsync(HttpMethod method, string titleId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(titleId);

        using var request = new HttpRequestMessage(method, $"public/titles/download/{titleId}.Title.Pack.gbx");
        return await Client.SendAsync(request, cancellationToken);
    }

    public virtual async Task<IngameTitle?> GetTitleByUidAsync(string uid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(uid);

        using var response = await Client.GetAsync($"public/titles/{uid}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ManiaPlanetIngameAPIJsonContext.Default.IngameTitle, cancellationToken);
    }
}
