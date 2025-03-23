using MinimalXmlReader;
using TmEssentials;

namespace ManiaAPI.XmlRpc.TMUF;

public interface IMasterServerTMUF : IMasterServer
{
    Task<LeagueRankings> GetLadderLeagueRankingsAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<LeagueRankings>> GetLadderLeagueRankingsResponseAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<PlayerRankings> GetLadderPlayerRankingsAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<PlayerRankings>> GetLadderPlayerRankingsResponseAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);

    Task<CampaignScores> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, int zoneId, CancellationToken cancellationToken = default);
    Task<CampaignScores?> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, string zone, CancellationToken cancellationToken = default);
    Task<GeneralScores> DownloadGeneralScoresAsync(ScoresNumber num, int zoneId, CancellationToken cancellationToken = default);
    Task<GeneralScores?> DownloadGeneralScoresAsync(ScoresNumber num, string zone, CancellationToken cancellationToken = default);
    Task<CampaignScores> DownloadLatestCampaignScoresAsync(string campaignName, int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<CampaignScores?> DownloadLatestCampaignScoresAsync(string campaignName, string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<GeneralScores> DownloadLatestGeneralScoresAsync(int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<GeneralScores?> DownloadLatestGeneralScoresAsync(string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestCampaignScoresInfoAsync(string campaignName, int zoneId, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestCampaignScoresInfoAsync(string campaignName, string zone, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestGeneralScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestGeneralScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestLadderScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestLadderScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default);
}

public class MasterServerTMUF : MasterServer, IMasterServerTMUF
{
    public const string DefaultAddress = "http://game.trackmaniaforever.com/online_game/request.php";

    private const string GeneralScoresName = "General";
    private const string LadderScoresName = "Multi";

    protected override string GameXml => "<version>2.11.25</version>";

    public MasterServerTMUF() : base(new Uri(DefaultAddress))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMUF"/> using any <see cref="HttpClient"/>. You need to set the base address yourself.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerTMUF(HttpClient client) : base(client)
    {
    }

    public virtual async Task<MasterServerResponse<PlayerRankings>> GetLadderPlayerRankingsResponseAsync(
        string zone = "World", 
        int page = 0, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetRankingsNew";
        var response = await XmlRpcHelper.SendAsync(Client, GameXml, RequestName, @$"
            <t>0</t>
            <st>g</st>
            <f>{zone}</f>
            <b>0</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var players = new List<PlayerRanking>();

            var playerCount = 0;

            while (xml.TryReadStartElement(out var element))
            {
                switch (element)
                {
                    case "c":
                        playerCount = int.Parse(xml.ReadContent());
                        break;
                    case "v":
                        var rank = 0;
                        var nickname = string.Empty;
                        var score = 0;
                        var leagueLogoUrl = string.Empty;

                        while (xml.TryReadStartElement(out var valueElement))
                        {
                            switch (valueElement)
                            {
                                case "g":
                                    rank = int.Parse(xml.ReadContent());
                                    break;
                                case "n":
                                    nickname = xml.ReadContentAsString();
                                    break;
                                case "h":
                                    score = int.Parse(xml.ReadContent());
                                    break;
                                case "i":
                                    leagueLogoUrl = xml.ReadContentAsString();
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        players.Add(new PlayerRanking(rank, nickname, score, leagueLogoUrl));
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            return new PlayerRankings(playerCount, players);
        });
    }

    public async Task<PlayerRankings> GetLadderPlayerRankingsAsync(
        string zone = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return (await GetLadderPlayerRankingsResponseAsync(zone, page, count, cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<LeagueRankings>> GetLadderLeagueRankingsResponseAsync(
        string zone = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetRankingsNew";
        var response = await XmlRpcHelper.SendAsync(Client, GameXml, RequestName, @$"
            <t>1</t>
            <st>g</st>
            <f>{zone}</f>
            <b>0</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var leagues = new List<LeagueRanking>();

            var leagueCount = 0;

            while (xml.TryReadStartElement(out var element))
            {
                switch (element)
                {
                    case "c":
                        leagueCount = int.Parse(xml.ReadContent());
                        break;
                    case "v":
                        var rank = 0;
                        var name = string.Empty;
                        var score = 0;
                        var playerCount = 0;

                        while (xml.TryReadStartElement(out var valueElement))
                        {
                            switch (valueElement)
                            {
                                case "g":
                                    rank = int.Parse(xml.ReadContent());
                                    break;
                                case "n":
                                    name = xml.ReadContentAsString();
                                    break;
                                case "h":
                                    score = int.Parse(xml.ReadContent());
                                    break;
                                case "l":
                                    playerCount = int.Parse(xml.ReadContent());
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        leagues.Add(new LeagueRanking(rank, name, score, playerCount));
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            return new LeagueRankings(leagueCount, leagues);
        });
    }

    public async Task<LeagueRankings> GetLadderLeagueRankingsAsync(
        string zone = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return (await GetLadderLeagueRankingsResponseAsync(zone, page, count, cancellationToken)).Result;
    }

    public virtual async Task<GeneralScores?> DownloadLatestGeneralScoresAsync(string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadLatestGeneralScoresAsync(zoneId, parallelLatestCheck, cancellationToken);
        }

        return null;
    }

    public virtual async Task<GeneralScores> DownloadLatestGeneralScoresAsync(int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        var scoresInfo = await FetchLatestGeneralScoresInfoAsync(zoneId, parallelLatestCheck, cancellationToken);

        return await DownloadGeneralScoresAsync(scoresInfo.Number, zoneId, cancellationToken);
    }

    public virtual async Task<GeneralScores?> DownloadGeneralScoresAsync(ScoresNumber num, string zone, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadGeneralScoresAsync(num, zoneId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<GeneralScores> DownloadGeneralScoresAsync(ScoresNumber num, int zoneId, CancellationToken cancellationToken = default)
    {
        var url = GetGeneralScoresUrl(num, zoneId);
        var response = await Client.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return GeneralScores.Parse(stream);
    }

    public virtual async Task<ScoresInfo?> FetchLatestGeneralScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, zone, parallel, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestGeneralScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, zoneId, parallel, cancellationToken);
    }

    internal static string GetGeneralScoresUrl(ScoresNumber num, int zoneId)
    {
        return GetScoresUrl(num, GeneralScoresName, zoneId);
    }

    /*public virtual async Task<LadderScores?> DownloadLatestLadderScoresAsync(string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        if (Zones.ZoneIdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadLatestLadderScoresAsync(zoneId, parallelLatestCheck, cancellationToken);
        }

        return null;
    }

    public virtual async Task<LadderScores> DownloadLatestLadderScoresAsync(int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        var scoresInfo = await FetchLatestLadderScoresInfoAsync(zoneId, parallelLatestCheck, cancellationToken);

        return await DownloadLadderScoresAsync(scoresInfo.Number, zoneId, cancellationToken);
    }

    public virtual async Task<LadderScores?> DownloadLadderScoresAsync(ScoresNumber num, string zone, CancellationToken cancellationToken = default)
    {
        if (Zones.ZoneIdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadLadderScoresAsync(num, zoneId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<LadderScores> DownloadLadderScoresAsync(ScoresNumber num, int zoneId, CancellationToken cancellationToken = default)
    {
        var url = GetLadderScoresUrl(num, zoneId);
        var response = await Client.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return LadderScores.Parse(stream);
    }*/

    public virtual async Task<ScoresInfo?> FetchLatestLadderScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(LadderScoresName, zone, parallel, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestLadderScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(LadderScoresName, zoneId, parallel, cancellationToken);
    }

    internal static string GetLadderScoresUrl(ScoresNumber num, int zoneId)
    {
        return GetScoresUrl(num, LadderScoresName, zoneId);
    }

    public virtual async Task<CampaignScores?> DownloadLatestCampaignScoresAsync(string campaignName, string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadLatestCampaignScoresAsync(campaignName, zoneId, parallelLatestCheck, cancellationToken);
        }

        return null;
    }

    public virtual async Task<CampaignScores> DownloadLatestCampaignScoresAsync(string campaignName, int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        var scoresInfo = await FetchLatestCampaignScoresInfoAsync(campaignName, zoneId, parallelLatestCheck, cancellationToken);

        return await DownloadCampaignScoresAsync(campaignName, scoresInfo.Number, zoneId, cancellationToken);
    }

    public virtual async Task<CampaignScores?> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, string zone, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadCampaignScoresAsync(campaignName, num, zoneId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<CampaignScores> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, int zoneId, CancellationToken cancellationToken = default)
    {
        var url = GetScoresUrl(num, campaignName, zoneId);
        var response = await Client.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return CampaignScores.Parse(stream);
    }

    public virtual async Task<ScoresInfo?> FetchLatestCampaignScoresInfoAsync(string campaignName, string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, zone, parallel, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestCampaignScoresInfoAsync(string campaignName, int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, zoneId, parallel, cancellationToken);
    }

    internal async Task<ScoresInfo?> FetchLatestScoresInfoAsync(string scoresName, string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
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
}
