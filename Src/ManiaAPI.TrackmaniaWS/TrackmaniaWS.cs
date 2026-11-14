using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

    /// <summary>
    /// Gets the nickname of a player formatted as HTML.
    /// </summary>
    Task<string> GetPlayerNicknameHtmlAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the tags of a player.
    /// </summary>
    Task<ImmutableList<PlayerTag>> GetPlayerTagsAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the multiplayer (ladder) ranking of a player.
    /// </summary>
    /// <param name="login">Login of the player.</param>
    /// <param name="environment">Environment, like <c>Stadium</c>. Null for all environments together (<c>Merge</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PlayerRanking> GetPlayerMultiplayerRankingAsync(string login, string? environment = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the solo (skill points) ranking of a player.
    /// </summary>
    Task<PlayerRanking> GetPlayerSoloRankingAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a zone by ID.
    /// </summary>
    Task<Zone> GetZoneAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a zone by path.
    /// </summary>
    /// <param name="path">Full path, like <c>World|France</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Zone> GetZoneByPathAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ID of a zone by path.
    /// </summary>
    /// <param name="path">Full path, like <c>World|France</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<int> GetZoneIdAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all zones.
    /// </summary>
    /// <param name="offset">Offset, starting from 0.</param>
    /// <param name="length">Number of zones, at most 100.</param>
    /// <param name="sort">Sorting, or null for the default one (by ID).</param>
    /// <param name="descending">Whether to sort in descending order. Applies only when <paramref name="sort"/> is set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImmutableList<Zone>> GetZonesAsync(int offset = 0, int length = 10, ZoneSort? sort = null, bool descending = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the child zones of a zone by ID.
    /// </summary>
    /// <param name="id">Zone ID.</param>
    /// <param name="offset">Offset, starting from 0.</param>
    /// <param name="length">Number of zones, at most 100.</param>
    /// <param name="sort">Sorting, or null for the default one (by ID).</param>
    /// <param name="descending">Whether to sort in descending order. Applies only when <paramref name="sort"/> is set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImmutableList<Zone>> GetZoneChildrenAsync(int id, int offset = 0, int length = 10, ZoneSort? sort = null, bool descending = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the child zones of a zone by path.
    /// </summary>
    /// <param name="path">Full path, like <c>World|France</c>.</param>
    /// <param name="offset">Offset, starting from 0.</param>
    /// <param name="length">Number of zones, at most 100.</param>
    /// <param name="sort">Sorting, or null for the default one (by ID).</param>
    /// <param name="descending">Whether to sort in descending order. Applies only when <paramref name="sort"/> is set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImmutableList<Zone>> GetZoneChildrenByPathAsync(string path, int offset = 0, int length = 10, ZoneSort? sort = null, bool descending = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the solo (skill points) leaderboard of players in a zone.
    /// </summary>
    /// <param name="path">Full zone path, like <c>World|France</c>. Null for the world.</param>
    /// <param name="offset">Offset, starting from 0.</param>
    /// <param name="length">Number of players.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PlayerLeaderboard> GetSoloLeaderboardAsync(string? path = null, int offset = 0, int length = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the solo leaderboard of players on a challenge (map) in a zone.
    /// </summary>
    /// <param name="challengeUid">UID of the challenge, like <c>BeySZdnfuSh4nHY5xztiXLmlrXe</c> (A01-Race).</param>
    /// <param name="path">Full zone path, like <c>World|France</c>. Null for the world.</param>
    /// <param name="offset">Offset, starting from 0.</param>
    /// <param name="length">Number of players.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ChallengeLeaderboard> GetSoloChallengeLeaderboardAsync(string challengeUid, string? path = null, int offset = 0, int length = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the multiplayer (ladder) leaderboard of players in a zone.
    /// </summary>
    /// <param name="path">Full zone path, like <c>World|France</c>.</param>
    /// <param name="environment">Environment, like <c>Stadium</c>, or <c>Merge</c> for all environments together.</param>
    /// <param name="offset">Offset, starting from 0.</param>
    /// <param name="length">Number of players.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PlayerLeaderboard> GetMultiplayerLeaderboardAsync(string path = "World", string environment = "Merge", int offset = 0, int length = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the multiplayer (ladder) leaderboard of the child zones of a zone.
    /// </summary>
    /// <param name="path">Full zone path, like <c>World|France</c>.</param>
    /// <param name="offset">Offset, starting from 0.</param>
    /// <param name="length">Number of zones.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ZoneLeaderboard> GetMultiplayerZoneLeaderboardAsync(string path = "World", int offset = 0, int length = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of registered TMF accounts.
    /// </summary>
    Task<int> GetRegisteredPlayerCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a short manialink code.
    /// </summary>
    Task<Manialink> GetManialinkAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a ManiaHome notification from a manialink to everyone following it.
    /// </summary>
    /// <param name="manialink">Manialink the notification is sent from. It has to belong to the API user.</param>
    /// <param name="message">Message.</param>
    /// <param name="link">Manialink to open from the notification.</param>
    /// <param name="iconStyle">Icon style.</param>
    /// <param name="iconSubStyle">Icon substyle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendManiaHomeNotificationAsync(string manialink, string message, string? link = null, string? iconStyle = null, string? iconSubStyle = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a public ManiaHome notification from a manialink to a player. It is visible to the buddies of the player.
    /// </summary>
    /// <param name="manialink">Manialink the notification is sent from. It has to belong to the API user.</param>
    /// <param name="login">Login of the player.</param>
    /// <param name="message">Message.</param>
    /// <param name="link">Manialink to open from the notification.</param>
    /// <param name="iconStyle">Icon style.</param>
    /// <param name="iconSubStyle">Icon substyle.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendManiaHomePublicNotificationAsync(string manialink, string login, string message, string? link = null, string? iconStyle = null, string? iconSubStyle = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a private ManiaHome notification from a manialink to a player.
    /// </summary>
    /// <param name="manialink">Manialink the notification is sent from. It has to belong to the API user.</param>
    /// <param name="login">Login of the player.</param>
    /// <param name="message">Message.</param>
    /// <param name="link">Manialink to open from the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendManiaHomePrivateNotificationAsync(string manialink, string login, string message, string? link = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks that the web services are up, using their public test route, which needs no credentials.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the service answers with its expected test response, otherwise false.</returns>
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}

public class TrackmaniaWS : ITrackmaniaWS
{
    public const string BaseAddress = "http://ws.trackmania.com/";

    private readonly AuthenticationHeaderValue authentication;

    public HttpClient Client { get; }

    public TrackmaniaWS(string apiUsername, string apiPassword, HttpClient client)
    {
        Client = client;

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(TrackmaniaWS; Email=petrpiv1@gmail.com; Discord=bigbang1112)"));
        }

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
        return await GetJsonAsync($"tmf/players/{Escape(login)}/", TrackmaniaWSJsonContext.Default.Player, cancellationToken);
    }

    public virtual async Task<string> GetPlayerNicknameHtmlAsync(string login, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/players/{Escape(login)}/nickname/html/", TrackmaniaWSJsonContext.Default.String, cancellationToken);
    }

    public virtual async Task<ImmutableList<PlayerTag>> GetPlayerTagsAsync(string login, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/players/{Escape(login)}/tags/", TrackmaniaWSJsonContext.Default.ImmutableListPlayerTag, cancellationToken);
    }

    public virtual async Task<PlayerRanking> GetPlayerMultiplayerRankingAsync(string login, string? environment = null, CancellationToken cancellationToken = default)
    {
        var endpoint = environment is null
            ? $"tmf/players/{Escape(login)}/rankings/multiplayer/"
            : $"tmf/players/{Escape(login)}/rankings/multiplayer/{Escape(environment)}/";

        return await GetJsonAsync(endpoint, TrackmaniaWSJsonContext.Default.PlayerRanking, cancellationToken);
    }

    public virtual async Task<PlayerRanking> GetPlayerSoloRankingAsync(string login, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/players/{Escape(login)}/rankings/solo/", TrackmaniaWSJsonContext.Default.PlayerRanking, cancellationToken);
    }

    public virtual async Task<Zone> GetZoneAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/zones/id/{id}/", TrackmaniaWSJsonContext.Default.Zone, cancellationToken);
    }

    public virtual async Task<Zone> GetZoneByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/zones/path/{Escape(path)}/", TrackmaniaWSJsonContext.Default.Zone, cancellationToken);
    }

    public virtual async Task<int> GetZoneIdAsync(string path, CancellationToken cancellationToken = default)
    {
        return await GetInt32Async($"tmf/zones/path/{Escape(path)}/id/", cancellationToken);
    }

    public virtual async Task<ImmutableList<Zone>> GetZonesAsync(int offset = 0, int length = 10, ZoneSort? sort = null, bool descending = false, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/zones/all/{ZoneQuery(offset, length, sort, descending)}", TrackmaniaWSJsonContext.Default.ImmutableListZone, cancellationToken);
    }

    public virtual async Task<ImmutableList<Zone>> GetZoneChildrenAsync(int id, int offset = 0, int length = 10, ZoneSort? sort = null, bool descending = false, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/zones/id/{id}/children/{ZoneQuery(offset, length, sort, descending)}", TrackmaniaWSJsonContext.Default.ImmutableListZone, cancellationToken);
    }

    public virtual async Task<ImmutableList<Zone>> GetZoneChildrenByPathAsync(string path, int offset = 0, int length = 10, ZoneSort? sort = null, bool descending = false, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/zones/path/{Escape(path)}/children/{ZoneQuery(offset, length, sort, descending)}", TrackmaniaWSJsonContext.Default.ImmutableListZone, cancellationToken);
    }

    public virtual async Task<PlayerLeaderboard> GetSoloLeaderboardAsync(string? path = null, int offset = 0, int length = 10, CancellationToken cancellationToken = default)
    {
        var endpoint = path is null
            ? $"tmf/rankings/solo/players/{PageQuery(offset, length)}"
            : $"tmf/rankings/solo/players/{Escape(path)}/{PageQuery(offset, length)}";

        return await GetJsonAsync(endpoint, TrackmaniaWSJsonContext.Default.PlayerLeaderboard, cancellationToken);
    }

    public virtual async Task<ChallengeLeaderboard> GetSoloChallengeLeaderboardAsync(string challengeUid, string? path = null, int offset = 0, int length = 10, CancellationToken cancellationToken = default)
    {
        var endpoint = path is null
            ? $"tmf/rankings/solo/challenges/{Escape(challengeUid)}/players/{PageQuery(offset, length)}"
            : $"tmf/rankings/solo/challenges/{Escape(challengeUid)}/players/{Escape(path)}/{PageQuery(offset, length)}";

        return await GetJsonAsync(endpoint, TrackmaniaWSJsonContext.Default.ChallengeLeaderboard, cancellationToken);
    }

    public virtual async Task<PlayerLeaderboard> GetMultiplayerLeaderboardAsync(string path = "World", string environment = "Merge", int offset = 0, int length = 10, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/rankings/multiplayer/players/{Escape(path)}/{Escape(environment)}/{PageQuery(offset, length)}", TrackmaniaWSJsonContext.Default.PlayerLeaderboard, cancellationToken);
    }

    public virtual async Task<ZoneLeaderboard> GetMultiplayerZoneLeaderboardAsync(string path = "World", int offset = 0, int length = 10, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/rankings/multiplayer/zones/{Escape(path)}/{PageQuery(offset, length)}", TrackmaniaWSJsonContext.Default.ZoneLeaderboard, cancellationToken);
    }

    public virtual async Task<int> GetRegisteredPlayerCountAsync(CancellationToken cancellationToken = default)
    {
        return await GetInt32Async("tmf/registrations/", cancellationToken);
    }

    public virtual async Task<Manialink> GetManialinkAsync(string code, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"tmf/manialinks/{Escape(code)}/", TrackmaniaWSJsonContext.Default.Manialink, cancellationToken);
    }

    public virtual async Task SendManiaHomeNotificationAsync(string manialink, string message, string? link = null, string? iconStyle = null, string? iconSubStyle = null, CancellationToken cancellationToken = default)
    {
        await PostJsonAsync("maniahome/notification/",
            new ManiaHomeNotification(manialink, ReceiverName: null, message, link, iconStyle, iconSubStyle), cancellationToken);
    }

    public virtual async Task SendManiaHomePublicNotificationAsync(string manialink, string login, string message, string? link = null, string? iconStyle = null, string? iconSubStyle = null, CancellationToken cancellationToken = default)
    {
        await PostJsonAsync($"maniahome/notification/{Escape(login)}/",
            new ManiaHomeNotification(manialink, login, message, link, iconStyle, iconSubStyle), cancellationToken);
    }

    public virtual async Task SendManiaHomePrivateNotificationAsync(string manialink, string login, string message, string? link = null, CancellationToken cancellationToken = default)
    {
        await PostJsonAsync($"maniahome/notification/{Escape(login)}/private/",
            new ManiaHomeNotification(manialink, login, message, link, IconStyle: null, IconSubStyle: null), cancellationToken);
    }

    public virtual async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        // The test route answers a fixed payload: ["GET", { "anInt": 1337, "aString": "quarante-deux", ... }]
        using var response = await Client.GetAsync("foobar/index.json", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        try
        {
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = document.RootElement;

            return root.ValueKind == JsonValueKind.Array && root.GetArrayLength() == 2 &&
                root[0].ValueKind == JsonValueKind.String && root[0].GetString() == "GET" &&
                root[1].ValueKind == JsonValueKind.Object &&
                root[1].TryGetProperty("anInt", out var anInt) && anInt.ValueKind == JsonValueKind.Number &&
                anInt.TryGetInt32(out var value) && value == 1337;
        }
        catch (JsonException)
        {
            // Something else answered, like an error page
            return false;
        }
    }

    protected internal async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = authentication;

        using var response = await Client.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new JsonException("The response body is null.");
    }

    private async Task PostJsonAsync(string endpoint, ManiaHomeNotification notification, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(notification, TrackmaniaWSJsonContext.Default.ManiaHomeNotification)
        };
        request.Headers.Authorization = authentication;

        using var response = await Client.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task<int> GetInt32Async(string endpoint, CancellationToken cancellationToken)
    {
        // These routes answer a number in a JSON string, like "310"
        var element = await GetJsonAsync(endpoint, TrackmaniaWSJsonContext.Default.JsonElement, cancellationToken);

        return element.ValueKind == JsonValueKind.String
            ? int.Parse(element.GetString()!, NumberStyles.Integer, CultureInfo.InvariantCulture)
            : element.GetInt32();
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        // Errors come as { "message": "..." }, but the message can be empty
        var message = default(string);

        try
        {
            message = (await response.Content.ReadFromJsonAsync(TrackmaniaWSJsonContext.Default.ErrorResponse, cancellationToken))?.Message;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // Not JSON, or a charset that can't be decoded, so only the status describes the error
        }

        if (string.IsNullOrEmpty(message))
        {
            message = $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).";
        }

        throw new TrackmaniaWSResponseException(message, response.StatusCode);
    }

    private static string Escape(string value) => Uri.EscapeDataString(value);

    private static string PageQuery(int offset, int length)
    {
        return string.Create(CultureInfo.InvariantCulture, $"?offset={offset}&length={length}");
    }

    private static string ZoneQuery(int offset, int length, ZoneSort? sort, bool descending)
    {
        var sortValue = sort switch
        {
            ZoneSort.Id => "id",
            ZoneSort.Name => "name",
            _ => ""
        };

        var order = sort is null ? "" : descending ? "-1" : "1";

        return PageQuery(offset, length) + $"&sort={sortValue}&order={order}";
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
