using ManiaAPI.Base.Converters;
using ManiaAPI.TrackmaniaIO.Converters;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManiaAPI.TrackmaniaIO;

public static class TrackmaniaIO
{
    internal static long? RateLimitRemaining;
    internal static DateTimeOffset? RateLimitReset;

    internal static JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    public static HttpClient Client { get; }

    static TrackmaniaIO()
    {
        JsonSerializerOptions.Converters.Add(new TimeInt32Converter());
        JsonSerializerOptions.Converters.Add(new CampaignItemConverter());
        JsonSerializerOptions.Converters.Add(new CampaignCollectionConverter());

        Client = new HttpClient
        {
            BaseAddress = new Uri("https://trackmania.io/api/")
        };

        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (TrackmaniaIO) by BigBang1112");
    }

    public static void AddUserAgent(string userAgent)
    {
        Client.DefaultRequestHeaders.Add("User-Agent", userAgent);
    }

    public static async Task<CampaignCollection> GetCampaignsAsync(int page = 0, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<CampaignCollection>($"campaigns/{page}", cancellationToken);
    }

    public static async Task<Campaign> GetCustomCampaignAsync(int clubId, int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<Campaign>($"campaign/{clubId}/{campaignId}", cancellationToken);
    }

    public static async Task<Campaign> GetOfficialCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<Campaign>($"officialcampaign/{campaignId}", cancellationToken);
    }

    public static async Task<Leaderboard> GetLeaderboardAsync(string leaderboardUid, string mapUid, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<Leaderboard>($"leaderboard/{leaderboardUid}/{mapUid}", cancellationToken);
    }

    public static async Task<WorldRecord[]> GetRecentWorldRecordsAsync(string leaderboardUid, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<WorldRecord[]>($"leaderboard/track/{leaderboardUid}", cancellationToken);
    }

    private static async Task<T> GetApiAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        if (RateLimitRemaining == 0)
        {
            while (DateTime.UtcNow < RateLimitReset)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
        
        using var response = await Client.GetAsync(requestUri, cancellationToken);

        response.EnsureSuccessStatusCode();

        RateLimitRemaining = GetHeaderNumberValue("X-Ratelimit-Remaining", response) ?? RateLimitRemaining - 1;

        var rateLimitResetCurrent = GetHeaderNumberValue("X-Ratelimit-Reset", response);

        if (rateLimitResetCurrent.HasValue)
        {
            RateLimitReset = DateTimeOffset.FromUnixTimeSeconds(rateLimitResetCurrent.Value);
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonSerializerOptions, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    internal static long? GetHeaderNumberValue(string name, HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(name, out IEnumerable<string>? values))
        {
            return null;
        }

        return long.TryParse(values.FirstOrDefault(), out long value) ? value : null;
    }
}
