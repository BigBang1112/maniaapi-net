using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.TrackmaniaIO;

public interface ITrackmaniaIO : IDisposable
{
    HttpClient Client { get; }

    Task<CampaignCollection> GetSeasonalCampaignsAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetClubCampaignsAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetWeeklyCampaignsAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<Campaign> GetSeasonalCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
    Task<Campaign> GetClubCampaignAsync(int clubId, int campaignId, CancellationToken cancellationToken = default);
    Task<Campaign> GetWeeklyCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
    Task<Leaderboard> GetLeaderboardAsync(string leaderboardUid, string mapUid, CancellationToken cancellationToken = default);
    Task<Leaderboard> GetLeaderboardAsync(string mapUid, int offset = 0, int length = 15, CancellationToken cancellationToken = default);
    Task<ImmutableList<WorldRecord>> GetRecentWorldRecordsAsync(string leaderboardUid, CancellationToken cancellationToken = default);
    Task<Map> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<ClubCollection> GetClubsAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<Club> GetClubAsync(int clubId, CancellationToken cancellationToken = default);
    Task<ClubMemberCollection> GetClubMembersAsync(int clubId, int page = 0, CancellationToken cancellationToken = default);
    Task<ClubActivityCollection> GetClubActivitiesAsync(int clubId, int page = 0, CancellationToken cancellationToken = default);
    Task<ClubRoomCollection> GetClubRoomsAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<ClubRoom> GetClubRoomAsync(int clubId, int roomId, CancellationToken cancellationToken = default);
    Task<TrackOfTheDayMonth> GetTrackOfTheDaysAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<AdCollection> GetAdsAsync(CancellationToken cancellationToken = default);
    Task<CompetitionCollection> GetCompetitionsAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<Competition> GetCompetitionAsync(int competitionId, CancellationToken cancellationToken = default);
}

public class TrackmaniaIO : ITrackmaniaIO
{
    internal static long? RateLimitRemaining;
    internal static DateTimeOffset? RateLimitReset;

    public const string BaseAddress = "https://trackmania.io/api";

    public HttpClient Client { get; }

    /// <summary>
    /// Make sure your user agent is descriptive enough, otherwise you might receive 403 Forbidden responses in the future.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="userAgent">Custom user agent as explained here: https://openplanet.dev/tmio/api</param>
    public TrackmaniaIO(HttpClient client, string userAgent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        Client = client ?? throw new ArgumentNullException(nameof(client));
        Client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NET/2.5.0 (TrackmaniaIO; Discord=bigbang1112)");
    }

    /// <summary>
    /// Make sure your user agent is descriptive enough, otherwise you might receive 403 Forbidden responses in the future.
    /// </summary>
    /// <param name="userAgent">Custom user agent as explained here: https://openplanet.dev/tmio/api</param>
    public TrackmaniaIO(string userAgent) : this(new HttpClient(), userAgent)
    {
        
    }

    public virtual async Task<CampaignCollection> GetSeasonalCampaignsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaigns/seasonal/{page}", TrackmaniaIOJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetClubCampaignsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaigns/club/{page}", TrackmaniaIOJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetWeeklyCampaignsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaigns/weekly/{page}", TrackmaniaIOJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<Campaign> GetSeasonalCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/seasonal/{campaignId}", TrackmaniaIOJsonContext.Default.Campaign, cancellationToken);
    }

    public virtual async Task<Campaign> GetClubCampaignAsync(int clubId, int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/{clubId}/{campaignId}", TrackmaniaIOJsonContext.Default.Campaign, cancellationToken);
    }

    public virtual async Task<Campaign> GetWeeklyCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/weekly/{campaignId}", TrackmaniaIOJsonContext.Default.Campaign, cancellationToken);
    }

    public virtual async Task<Leaderboard> GetLeaderboardAsync(string leaderboardUid, string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(leaderboardUid);
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"leaderboard/{leaderboardUid}/{mapUid}", TrackmaniaIOJsonContext.Default.Leaderboard, cancellationToken);
    }

    public virtual async Task<Leaderboard> GetLeaderboardAsync(string mapUid, int offset = 0, int length = 15, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"leaderboard/map/{mapUid}?offset={offset}&length={length}", TrackmaniaIOJsonContext.Default.Leaderboard, cancellationToken);
    }

    public virtual async Task<ImmutableList<WorldRecord>> GetRecentWorldRecordsAsync(string leaderboardUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(leaderboardUid);

        return await GetJsonAsync($"leaderboard/track/{leaderboardUid}", TrackmaniaIOJsonContext.Default.ImmutableListWorldRecord, cancellationToken);
    }

    public virtual async Task<Map> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"map/{mapUid}", TrackmaniaIOJsonContext.Default.Map, cancellationToken);
    }

    public virtual async Task<ClubCollection> GetClubsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"clubs/{page}", TrackmaniaIOJsonContext.Default.ClubCollection, cancellationToken);
    }

    public virtual async Task<Club> GetClubAsync(int clubId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"club/{clubId}", TrackmaniaIOJsonContext.Default.Club, cancellationToken);
    }

    public virtual async Task<ClubMemberCollection> GetClubMembersAsync(int clubId, int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"club/{clubId}/members/{page}", TrackmaniaIOJsonContext.Default.ClubMemberCollection, cancellationToken);
    }

    public virtual async Task<ClubActivityCollection> GetClubActivitiesAsync(int clubId, int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"club/{clubId}/activities/{page}", TrackmaniaIOJsonContext.Default.ClubActivityCollection, cancellationToken);
    }

    public virtual async Task<ClubRoomCollection> GetClubRoomsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"rooms/{page}", TrackmaniaIOJsonContext.Default.ClubRoomCollection, cancellationToken);
    }

    public virtual async Task<ClubRoom> GetClubRoomAsync(int clubId, int roomId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"room/{clubId}/{roomId}", TrackmaniaIOJsonContext.Default.ClubRoom, cancellationToken);
    }

    public virtual async Task<TrackOfTheDayMonth> GetTrackOfTheDaysAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"totd/{page}", TrackmaniaIOJsonContext.Default.TrackOfTheDayMonth, cancellationToken);
    }

    public virtual async Task<AdCollection> GetAdsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("ads", TrackmaniaIOJsonContext.Default.AdCollection, cancellationToken);
    }

    public virtual async Task<CompetitionCollection> GetCompetitionsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"competitions/{page}", TrackmaniaIOJsonContext.Default.CompetitionCollection, cancellationToken);
    }

    public virtual async Task<Competition> GetCompetitionAsync(int competitionId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"comp/{competitionId}", TrackmaniaIOJsonContext.Default.Competition, cancellationToken);
    }

    protected internal async Task<T> GetJsonAsync<T>(string? endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        if (RateLimitRemaining == 0)
        {
            while (DateTime.UtcNow < RateLimitReset)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        using var response = await Client.GetAsync($"{BaseAddress}/{endpoint}", cancellationToken);

        response.EnsureSuccessStatusCode();

        RateLimitRemaining = GetHeaderNumberValue("X-Ratelimit-Remaining", response) ?? RateLimitRemaining - 1;

        var rateLimitResetCurrent = GetHeaderNumberValue("X-Ratelimit-Reset", response);

        if (rateLimitResetCurrent.HasValue)
        {
            RateLimitReset = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(rateLimitResetCurrent.Value);
        }

        Debug.WriteLine($"Route: {endpoint}{Environment.NewLine}Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    internal static long? GetHeaderNumberValue(string name, HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(name, out IEnumerable<string>? values))
        {
            return null;
        }

        return long.TryParse(values.FirstOrDefault(), out long value) ? value : null;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
