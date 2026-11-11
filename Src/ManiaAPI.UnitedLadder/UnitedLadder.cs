using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.UnitedLadder;

public interface IUnitedLadder : IDisposable
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

public class UnitedLadder : IUnitedLadder
{
    public const string BaseAddress = "https://api.ul.unitedascenders.xyz";

    public HttpClient Client { get; }

    public UnitedLadder(HttpClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(UnitedLadder; Discord=bigbang1112)"));
        }
    }

    public UnitedLadder() : this(new HttpClient())
    {

    }

    public virtual async Task<Player> GetPlayerAsync(string login, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"player/{Uri.EscapeDataString(login)}", UnitedLadderJsonContext.Default.Player, cancellationToken);
    }

    protected internal async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/{endpoint}");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
