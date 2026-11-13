using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManiaAPI.ManiaPlanetAPI;

public class ManiaPlanetChannelFeedbackAPI : IManiaPlanetChannelFeedbackAPI
{
    public const string BaseAddress = "https://feedback.prod.live.maniaplanet.com/";

    public HttpClient Client { get; }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet Channel Feedback API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public ManiaPlanetChannelFeedbackAPI(HttpClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(ManiaPlanetChannelFeedbackAPI; Email=petrpiv1@gmail.com; Discord=bigbang1112)"));
        }
    }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet Channel Feedback API client.
    /// </summary>
    public ManiaPlanetChannelFeedbackAPI() : this(new HttpClient { BaseAddress = new Uri(BaseAddress) }) { }

    public virtual async Task<ChannelFeedbackStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync("status", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(ManiaPlanetChannelFeedbackAPIJsonContext.Default.ChannelFeedbackStatus, cancellationToken)
            ?? throw new JsonException("The response body is null.");
    }
}
