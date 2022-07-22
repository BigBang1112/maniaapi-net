using TmEssentials;

namespace ManiaAPI.XmlRpc.TMUF;

public class MasterServerTMUF : MasterServer
{
    private const string GeneralScoresName = "General";

    public async Task<GeneralScores?> DownloadLatestGeneralScoresAsync(string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        if (Zones.ZoneIdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadLatestGeneralScoresAsync(zoneId, parallelLatestCheck, cancellationToken);
        }

        return null;
    }

    public async Task<GeneralScores> DownloadLatestGeneralScoresAsync(int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        var scoresInfo = await FetchLatestGeneralScoresInfoAsync(zoneId, parallelLatestCheck, cancellationToken);

        return await DownloadGeneralScoresAsync(scoresInfo.Number, zoneId, cancellationToken);
    }

    public async Task<GeneralScores?> DownloadGeneralScoresAsync(ScoresNumber num, string zone, CancellationToken cancellationToken = default)
    {
        if (Zones.ZoneIdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadGeneralScoresAsync(num, zoneId, cancellationToken);
        }

        return null;
    }

    public async Task<GeneralScores> DownloadGeneralScoresAsync(ScoresNumber num, int zoneId, CancellationToken cancellationToken = default)
    {
        var url = GetGeneralScoresUrl(num, zoneId);
        var response = await Client.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        return GeneralScores.Parse(stream);
    }

    public async Task<ScoresInfo?> FetchLatestCampaignScoresInfoAsync(string campaignName, string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, zone, parallel, cancellationToken);
    }

    public async Task<ScoresInfo> FetchLatestCampaignScoresInfoAsync(string campaignName, int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, zoneId, parallel, cancellationToken);
    }
    
    public async Task<ScoresInfo?> FetchLatestGeneralScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, zone, parallel, cancellationToken);
    }

    public async Task<ScoresInfo> FetchLatestGeneralScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, zoneId, parallel, cancellationToken);
    }

    internal async Task<ScoresInfo?> FetchLatestScoresInfoAsync(string scoresName, string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        if (Zones.ZoneIdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await FetchLatestScoresInfoAsync(scoresName, zoneId, parallel, cancellationToken);
        }

        return null;
    }

    internal async Task<ScoresInfo> FetchLatestScoresInfoAsync(string scoresName, int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        var responses = new Dictionary<ScoresNumber, Task<HttpResponseMessage>>();

        foreach (var num in Enum.GetValues<ScoresNumber>())
        {
            var url = GetScoresUrl(num, scoresName, zoneId);
            var task = Client.HeadAsync(url, cancellationToken);

            responses.Add(num, task);

            if (!parallel)
            {
                await task;
            }
        }

        await Task.WhenAll(responses.Values);

        if (responses.All(x => !x.Value.Result.IsSuccessStatusCode))
        {
            throw new Exception($"Zone ID {zoneId} is not available.");
        }

        var latestModified = responses
            .Where(x => x.Value.Result.Content.Headers.LastModified.HasValue)
            .Max(x => x.Value.Result.Content.Headers.LastModified) ?? throw new Exception("Last modified is null");
        
        foreach (var (num, response) in responses)
        {
            if (response.Result.Content.Headers.LastModified == latestModified)
            {
                return new(latestModified, num);
            }
        }

        throw new Exception();
    }

    internal static string GetScoresUrl(ScoresNumber num, string scoresName, int zoneId)
    {
        return $"http://scores.trackmaniaforever.com/scores{(int)num}/{scoresName}/{scoresName}{zoneId}.gz";
    }

    internal static string GetGeneralScoresUrl(ScoresNumber num, int zoneId)
    {
        return GetScoresUrl(num, GeneralScoresName, zoneId);
    }
}
