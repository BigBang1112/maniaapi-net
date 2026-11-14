using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManiaAPI.ManiaPlanetAPI;

public class ManiaPlanetChannelRecordAPI : IManiaPlanetChannelRecordAPI
{
    public const string BaseAddress = "https://records.prod.live.maniaplanet.com/";

    /// <summary>
    /// The leaderboard type of channel maps.
    /// </summary>
    public const string ChannelType = "channel";

    /// <summary>
    /// Most records <see cref="GetRecordsAsync"/> can get at once.
    /// </summary>
    public const int MaxLength = 101;

    // The API answers this for an unknown map, an unknown type, and a range past the last record
    private const string NoLeaderboardMessage = "No leaderboard for this map";

    public HttpClient Client { get; }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet Channel Record API client.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public ManiaPlanetChannelRecordAPI(HttpClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = PackageInfo.Version;

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(ManiaPlanetChannelRecordAPI; Email=petrpiv1@gmail.com; Discord=bigbang1112)"));
        }
    }

    /// <summary>
    /// Creates a new instance of the ManiaPlanet Channel Record API client.
    /// </summary>
    public ManiaPlanetChannelRecordAPI() : this(new HttpClient { BaseAddress = new Uri(BaseAddress) }) { }

    public virtual async Task<ChannelPlayerRecord?> GetPlayerRecordAsync(string mapUid, string login, string type = ChannelType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);
        ArgumentException.ThrowIfNullOrEmpty(login);
        ArgumentException.ThrowIfNullOrEmpty(type);

        using var response = await Client.GetAsync(
            $"leaderboard/{UrlHelper.EscapePathSegment(mapUid)}/user/{UrlHelper.EscapePathSegment(login)}/{UrlHelper.EscapePathSegment(type)}",
            cancellationToken);

        // An unknown map or type has no body
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync(ManiaPlanetChannelRecordAPIJsonContext.Default.ChannelPlayerRecordResponse, cancellationToken)
            ?? throw new JsonException("The response body is null.");

        var record = result is { UserLogin: not null, Rank: not null, Score: not null }
            ? new ChannelRecord(result.UserLogin, result.UserNickname ?? result.UserLogin, result.Rank.Value, result.Score.Value)
            : null;

        return new ChannelPlayerRecord(record, result.Length);
    }

    public virtual async Task<ChannelLeaderboard?> GetLeaderboardAsync(string mapUid, IEnumerable<string>? logins = null, string type = ChannelType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);
        ArgumentException.ThrowIfNullOrEmpty(type);

        // The list is required, even when empty
        var request = new ChannelLeaderboardRequest(mapUid, logins?.ToArray() ?? [], type);

        using var response = await Client.PostAsJsonAsync("leaderboard/get", request,
            ManiaPlanetChannelRecordAPIJsonContext.Default.ChannelLeaderboardRequest, cancellationToken);

        if (await IsNoLeaderboardAsync(response, cancellationToken))
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync(ManiaPlanetChannelRecordAPIJsonContext.Default.ChannelLeaderboardResponse, cancellationToken)
            ?? throw new JsonException("The response body is null.");

        return new ChannelLeaderboard(
            Ordered(result.Ranking),
            result.Length,
            result.ListPlayer?.ToImmutableDictionary() ?? ImmutableDictionary<string, int>.Empty);
    }

    public virtual async Task<IReadOnlyList<ChannelRecord>> GetRecordsAsync(string mapUid, int offset = 0, int length = 100, string type = ChannelType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, MaxLength);

        // Ranks are inclusive and start at 1; the API rejects ranges longer than 101 records
        var request = new ChannelRecordsRequest(mapUid, offset + 1, offset + length, type);

        using var response = await Client.PostAsJsonAsync("leaderboard/getrange", request,
            ManiaPlanetChannelRecordAPIJsonContext.Default.ChannelRecordsRequest, cancellationToken);

        if (await IsNoLeaderboardAsync(response, cancellationToken))
        {
            return [];
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync(ManiaPlanetChannelRecordAPIJsonContext.Default.ChannelRecordsResponse, cancellationToken)
            ?? throw new JsonException("The response body is null.");

        return Ordered(result.Ranking);
    }

    private static ImmutableList<ChannelRecord> Ordered(Dictionary<int, ChannelRecord>? ranking)
    {
        return ranking?.OrderBy(x => x.Key).Select(x => x.Value).ToImmutableList() ?? [];
    }

    private static async Task<bool> IsNoLeaderboardAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            return false;
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);

        return message.Trim().Trim('"') == NoLeaderboardMessage;
    }
}
