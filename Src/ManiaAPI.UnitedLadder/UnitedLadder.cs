using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    /// <summary>Gets a player, optionally bypassing stored history and fetching from TMF Web Services.</summary>
    Task<Player> GetPlayerAsync(string login, bool forceWebServices, CancellationToken cancellationToken = default);

    /// <summary>Gets official solo rankings for a zone, up to 100 players per zero-based page.</summary>
    Task<ImmutableList<PlayerRanking>> GetSoloRankingsAsync(string zone = "World", int page = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets multiplayer rankings. Environment is Stadium, Island, Desert, Rally, Bay, Coast, Snow, Mixed, or Merge; pages are zero-based.</summary>
    Task<ImmutableList<PlayerRanking>> GetMultiplayerRankingsAsync(string environment = "Merge", string zone = "World", int page = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets a track's official leaderboard. Pages are zero-based.</summary>
    Task<TrackLeaderboard> GetTrackLeaderboardAsync(string uid, string zone = "World", int page = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets a player's official map results. Pages are zero-based.</summary>
    Task<ImmutableList<MapPoints>> GetPlayerMapsAsync(string login, int page = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets official solo statistics by environment and campaign.</summary>
    Task<SoloBreakdown> GetPlayerSoloBreakdownAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>Checks whether a login is registered and has United status.</summary>
    Task<LoginData> CheckLoginAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>Gets historical player data. Omitted dates use the server's full available range.</summary>
    Task<ImmutableList<HistoricalPlayerData>> GetPlayerHistoryAsync(string login, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets progression for skillpoints, ladderpoints, or nickname. Environment selects the ladder points environment.</summary>
    Task<ImmutableList<ProgressionDataPoint>> GetPlayerProgressionAsync(string login, string field = "skillpoints", string? environment = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a snapshot retained for 24 hours. Type is solo, multi, or track; environment is solo, an environment name, or a track UID, respectively. Pages are zero-based.</summary>
    Task<ImmutableList<LeaderboardSnapshot>> GetLeaderboardSnapshotAsync(string type, string zone = "World", string? environment = null, int? page = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a track ghost replay using the numeric player ID, not the login. The caller disposes the response.</summary>
    Task<HttpResponseMessage> GetReplayResponseAsync(string uid, int playerId, CancellationToken cancellationToken = default);
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
        return await GetJsonAsync($"player/{Escape(login, nameof(login))}", UnitedLadderJsonContext.Default.Player, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<Player> GetPlayerAsync(string login, bool forceWebServices, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"player/{Escape(login, nameof(login))}?ws={(forceWebServices ? "true" : "false")}", UnitedLadderJsonContext.Default.Player, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<ImmutableList<PlayerRanking>> GetSoloRankingsAsync(string zone = "World", int page = 0, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        return await GetJsonAsync($"solo/{Escape(zone, nameof(zone))}/{page.ToString(CultureInfo.InvariantCulture)}", UnitedLadderJsonContext.Default.ImmutableListPlayerRanking, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<ImmutableList<PlayerRanking>> GetMultiplayerRankingsAsync(string environment = "Merge", string zone = "World", int page = 0, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        return await GetJsonAsync($"multi/{Escape(zone, nameof(zone))}/{Escape(environment, nameof(environment))}/{page.ToString(CultureInfo.InvariantCulture)}", UnitedLadderJsonContext.Default.ImmutableListPlayerRanking, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TrackLeaderboard> GetTrackLeaderboardAsync(string uid, string zone = "World", int page = 0, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        return await GetJsonAsync($"tracks/{Escape(zone, nameof(zone))}/{Escape(uid, nameof(uid))}/{page.ToString(CultureInfo.InvariantCulture)}", UnitedLadderJsonContext.Default.TrackLeaderboard, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<ImmutableList<MapPoints>> GetPlayerMapsAsync(string login, int page = 0, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        return await GetJsonAsync($"solomaps/{Escape(login, nameof(login))}/{page.ToString(CultureInfo.InvariantCulture)}", UnitedLadderJsonContext.Default.ImmutableListMapPoints, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<SoloBreakdown> GetPlayerSoloBreakdownAsync(string login, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"solobreakdown/{Escape(login, nameof(login))}", UnitedLadderJsonContext.Default.SoloBreakdown, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<LoginData> CheckLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"checklogin/{Escape(login, nameof(login))}", UnitedLadderJsonContext.Default.LoginData, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<ImmutableList<HistoricalPlayerData>> GetPlayerHistoryAsync(string login, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(startDate, endDate);
        var query = Query(("startDate", FormatDate(startDate)), ("endDate", FormatDate(endDate)));
        return await GetJsonAsync($"history/{Escape(login, nameof(login))}{query}", UnitedLadderJsonContext.Default.ImmutableListHistoricalPlayerData, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<ImmutableList<ProgressionDataPoint>> GetPlayerProgressionAsync(string login, string field = "skillpoints", string? environment = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(field);
        ValidateDateRange(startDate, endDate);
        var query = Query(("field", field), ("envi", environment), ("startDate", FormatDate(startDate)), ("endDate", FormatDate(endDate)));
        return await GetJsonAsync($"progression/{Escape(login, nameof(login))}{query}", UnitedLadderJsonContext.Default.ImmutableListProgressionDataPoint, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<ImmutableList<LeaderboardSnapshot>> GetLeaderboardSnapshotAsync(string type, string zone = "World", string? environment = null, int? page = null, CancellationToken cancellationToken = default)
    {
        if (page.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(page.Value, nameof(page));
        }

        var query = Query(("envi", environment), ("page", page?.ToString(CultureInfo.InvariantCulture)));
        return await GetJsonAsync($"snapshot/{Escape(type, nameof(type))}/{Escape(zone, nameof(zone))}{query}", UnitedLadderJsonContext.Default.ImmutableListLeaderboardSnapshot, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<HttpResponseMessage> GetReplayResponseAsync(string uid, int playerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfNegative(playerId);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/replays/{Escape(uid, nameof(uid))}/{playerId.ToString(CultureInfo.InvariantCulture)}");
        var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        try
        {
            response.EnsureSuccessStatusCode();
            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static string Escape(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, parameterName);
        return Uri.EscapeDataString(value);
    }

    private static string? FormatDate(DateTimeOffset? date) => date?.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);

    private static void ValidateDateRange(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("The end date must not precede the start date.", nameof(endDate));
        }
    }

    private static string Query(params (string Name, string? Value)[] parameters)
    {
        var query = string.Join("&", parameters.Where(p => p.Value is not null).Select(p => $"{p.Name}={Uri.EscapeDataString(p.Value!)}"));
        return query.Length == 0 ? "" : $"?{query}";
    }

    protected internal async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/{endpoint}");

        using var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new JsonException("The response body is null.");
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
