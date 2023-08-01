using ManiaAPI.TrackmaniaIO.JsonContexts;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.TrackmaniaIO;

public interface ITrackmaniaIO : IDisposable
{
    HttpClient Client { get; }

    Task<CampaignCollection> GetCampaignsAsync(int page = 0, CancellationToken cancellationToken = default);
    Task<Campaign> GetCustomCampaignAsync(int clubId, int campaignId, CancellationToken cancellationToken = default);
    Task<Leaderboard> GetLeaderboardAsync(string leaderboardUid, string mapUid, CancellationToken cancellationToken = default);
    Task<Campaign> GetOfficialCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
    Task<WorldRecord[]> GetRecentWorldRecordsAsync(string leaderboardUid, CancellationToken cancellationToken = default);
}

public class TrackmaniaIO : ITrackmaniaIO
{
    internal static long? RateLimitRemaining;
    internal static DateTimeOffset? RateLimitReset;

    public HttpClient Client { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client">HTTP client.</param>
    /// <param name="userAgent">Custom user agent as explained in https://openplanet.dev/tmio/api</param>
    public TrackmaniaIO(HttpClient client, string userAgent)
    {
        Client = client;
        Client.BaseAddress = new Uri("https://trackmania.io/api/");

        Client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TrackmaniaIO) by BigBang1112");
    }

    public TrackmaniaIO(string userAgent) : this(new HttpClient(), userAgent)
    {
        
    }

    public virtual async Task<CampaignCollection> GetCampaignsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaigns/{page}", TrackmaniaIOJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<Campaign> GetCustomCampaignAsync(int clubId, int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/{clubId}/{campaignId}", TrackmaniaIOJsonContext.Default.Campaign, cancellationToken);
    }

    public virtual async Task<Campaign> GetOfficialCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"officialcampaign/{campaignId}", TrackmaniaIOJsonContext.Default.Campaign, cancellationToken);
    }

    public virtual async Task<Leaderboard> GetLeaderboardAsync(string leaderboardUid, string mapUid, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"leaderboard/{leaderboardUid}/{mapUid}", TrackmaniaIOJsonContext.Default.Leaderboard, cancellationToken);
    }

    public virtual async Task<WorldRecord[]> GetRecentWorldRecordsAsync(string leaderboardUid, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"leaderboard/track/{leaderboardUid}", TrackmaniaIOJsonContext.Default.WorldRecordArray, cancellationToken);
    }

    protected internal async Task<T> GetJsonAsync<T>(string endpoint, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
    {
        if (RateLimitRemaining == 0)
        {
            while (DateTime.UtcNow < RateLimitReset)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        using var response = await Client.GetAsync(endpoint, cancellationToken);

        response.EnsureSuccessStatusCode();

        RateLimitRemaining = GetHeaderNumberValue("X-Ratelimit-Remaining", response) ?? RateLimitRemaining - 1;

        var rateLimitResetCurrent = GetHeaderNumberValue("X-Ratelimit-Reset", response);

        if (rateLimitResetCurrent.HasValue)
        {
            RateLimitReset = DateTimeOffset.FromUnixTimeSeconds(rateLimitResetCurrent.Value);
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
