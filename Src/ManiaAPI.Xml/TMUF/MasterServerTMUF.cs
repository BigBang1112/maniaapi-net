using MinimalXmlReader;
using System.Collections.Immutable;
using System.Globalization;
using TmEssentials;

namespace ManiaAPI.Xml.TMUF;

public interface IMasterServerTMUF : IMasterServer
{
    Task<LeagueRankings> GetLadderLeagueRankingsAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<LeagueRankings>> GetLadderLeagueRankingsResponseAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<PlayerRankings> GetLadderPlayerRankingsAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<PlayerRankings>> GetLadderPlayerRankingsResponseAsync(string zone = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<PlayerAchievements> GetPlayerAchievementsAsync(string login, int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<PlayerAchievements>> GetPlayerAchievementsResponseAsync(string login, int page = 0, int count = 10, CancellationToken cancellationToken = default);

    Task<CampaignScores> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, int zoneId, CancellationToken cancellationToken = default);
    Task<CampaignScores?> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, string zone, CancellationToken cancellationToken = default);
    Task<GeneralScores> DownloadGeneralScoresAsync(ScoresNumber num, int zoneId, CancellationToken cancellationToken = default);
    Task<GeneralScores?> DownloadGeneralScoresAsync(ScoresNumber num, string zone, CancellationToken cancellationToken = default);
    Task<LadderScores> DownloadLadderScoresAsync(ScoresNumber num, int zoneId, CancellationToken cancellationToken = default);
    Task<LadderScores?> DownloadLadderScoresAsync(ScoresNumber num, string zone, CancellationToken cancellationToken = default);
    Task<CampaignScores> DownloadLatestCampaignScoresAsync(string campaignName, int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<CampaignScores?> DownloadLatestCampaignScoresAsync(string campaignName, string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<GeneralScores> DownloadLatestGeneralScoresAsync(int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<GeneralScores?> DownloadLatestGeneralScoresAsync(string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<LadderScores> DownloadLatestLadderScoresAsync(int zoneId, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<LadderScores?> DownloadLatestLadderScoresAsync(string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default);
    Task<Stream> DownloadScoresAsync(ScoresNumber num, string scoresName, int zoneId, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, string zone, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchGeneralScoresDateTimeAsync(ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> FetchGeneralScoresDateTimeAsync(ScoresNumber num, string zone, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchLadderScoresDateTimeAsync(ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> FetchLadderScoresDateTimeAsync(ScoresNumber num, string zone, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchScoresDateTimeAsync(string scoresName, ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestCampaignScoresInfoAsync(string campaignName, int zoneId, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestCampaignScoresInfoAsync(string campaignName, string zone, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestGeneralScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestGeneralScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestLadderScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestLadderScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default);

    static abstract string GetGeneralScoresUrl(ScoresNumber num, int zoneId);
    static abstract string GetLadderScoresUrl(ScoresNumber num, int zoneId);
    static abstract string GetScoresUrl(ScoresNumber num, string scoresName, int zoneId);
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
    /// Creates a new instance of <see cref="MasterServerTMUF"/> using any <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerTMUF(HttpClient client) : base(new Uri(DefaultAddress), client)
    {
    }

    public virtual async Task<MasterServerResponse<PlayerRankings>> GetLadderPlayerRankingsResponseAsync(
        string zone = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetRankingsNew";
        var response = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml: null, RequestName, @$"
            <t>0</t>
            <st>g</st>
            <f>{zone}</f>
            <b>0</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var players = ImmutableList.CreateBuilder<PlayerRanking>();

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

            return new PlayerRankings(playerCount, players.ToImmutable());
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
        var response = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml: null, RequestName, @$"
            <t>1</t>
            <st>g</st>
            <f>{zone}</f>
            <b>0</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var leagues = ImmutableList.CreateBuilder<LeagueRanking>();

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

            return new LeagueRankings(leagueCount, leagues.ToImmutable());
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

    public virtual async Task<MasterServerResponse<PlayerAchievements>> GetPlayerAchievementsResponseAsync(string login, int page = 0, int count = 10, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetRankingsNew";
        var response = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml: null, RequestName, @$"
            <t>4</t>
            <st>-1||-1</st>
            <f>{login}</f>
            <b>1</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var achievements = ImmutableList.CreateBuilder<PlayerAchievement>();
            var achievementCount = 0;
            var aa = DateTimeOffset.MinValue;

            while (xml.TryReadStartElement(out var element))
            {
                switch (element)
                {
                    case "c":
                        achievementCount = int.Parse(xml.ReadContent());
                        break;
                    case "v":
                        var mapUid = string.Empty;
                        var skillpoints = 0;
                        var name = string.Empty;
                        var d = 0;
                        var m = 0;
                        var environment = string.Empty;
                        var dateAchieved = DateTimeOffset.MinValue;

                        while (xml.TryReadStartElement(out var valueElement))
                        {
                            switch (valueElement)
                            {
                                case "u":
                                    mapUid = xml.ReadContentAsString();
                                    break;
                                case "x":
                                    skillpoints = int.Parse(xml.ReadContent());
                                    break;
                                case "n":
                                    name = xml.ReadContentAsString();
                                    break;
                                case "d":
                                    d = int.Parse(xml.ReadContent());
                                    break;
                                case "m":
                                    m = int.Parse(xml.ReadContent());
                                    break;
                                case "e":
                                    environment = xml.ReadContentAsString();
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }
                            _ = xml.SkipEndElement();
                        }

                        achievements.Add(new PlayerAchievement(mapUid, skillpoints, name, d, m, environment));
                        break;
                    case "t":
                        if (!MemoryExtensions.Equals(xml.ReadContent(), "Achievements", StringComparison.Ordinal))
                        {
                            throw new Exception("Expected 'Achievements' in <t> element.");
                        }
                        break;
                    case "aa":
                        aa = DateTimeOffset.ParseExact(xml.ReadContent(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }
                _ = xml.SkipEndElement();
            }

            return new PlayerAchievements(achievementCount, aa, achievements.ToImmutable());
        });
    }

    public async Task<PlayerAchievements> GetPlayerAchievementsAsync(string login, int page = 0, int count = 10, CancellationToken cancellationToken = default)
    {
        return (await GetPlayerAchievementsResponseAsync(login, page, count, cancellationToken)).Result;
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

    public virtual async Task<LadderScores?> DownloadLatestLadderScoresAsync(string zone, bool parallelLatestCheck = false, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
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
        using var stream = await DownloadScoresAsync(num, GeneralScoresName, zoneId, cancellationToken);
        return GeneralScores.Parse(stream);
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
        using var stream = await DownloadScoresAsync(num, campaignName, zoneId, cancellationToken);
        return CampaignScores.Parse(stream);
    }

    public virtual async Task<LadderScores> DownloadLadderScoresAsync(ScoresNumber num, int zoneId, CancellationToken cancellationToken = default)
    {
        using var stream = await DownloadScoresAsync(num, LadderScoresName, zoneId, cancellationToken);
        return LadderScores.Parse(stream);
    }

    public virtual async Task<LadderScores?> DownloadLadderScoresAsync(ScoresNumber num, string zone, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await DownloadLadderScoresAsync(num, zoneId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<ScoresInfo?> FetchLatestGeneralScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, zone, parallel, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestGeneralScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, zoneId, parallel, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo?> FetchLatestLadderScoresInfoAsync(string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(LadderScoresName, zone, parallel, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestLadderScoresInfoAsync(int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(LadderScoresName, zoneId, parallel, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo?> FetchLatestCampaignScoresInfoAsync(string campaignName, string zone, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, zone, parallel, scores7: true, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestCampaignScoresInfoAsync(string campaignName, int zoneId, bool parallel = false, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, zoneId, parallel, scores7: true, cancellationToken);
    }

    public async Task<DateTimeOffset> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        return await FetchScoresDateTimeAsync(campaignName, num, zoneId, lastModified, cancellationToken);
    }

    public async Task<DateTimeOffset?> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, string zone, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await FetchCampaignScoresDateTimeAsync(campaignName, num, zoneId, lastModified, cancellationToken);
        }

        return null;
    }

    public async Task<DateTimeOffset> FetchGeneralScoresDateTimeAsync(ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        return await FetchScoresDateTimeAsync(GeneralScoresName, num, zoneId, lastModified, cancellationToken);
    }

    public async Task<DateTimeOffset?> FetchGeneralScoresDateTimeAsync(ScoresNumber num, string zone, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await FetchGeneralScoresDateTimeAsync(num, zoneId, lastModified, cancellationToken);
        }

        return null;
    }

    public async Task<DateTimeOffset> FetchLadderScoresDateTimeAsync(ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        return await FetchScoresDateTimeAsync(LadderScoresName, num, zoneId, lastModified, cancellationToken);
    }

    public async Task<DateTimeOffset?> FetchLadderScoresDateTimeAsync(ScoresNumber num, string zone, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await FetchLadderScoresDateTimeAsync(num, zoneId, lastModified, cancellationToken);
        }

        return null;
    }

    public virtual async Task<Stream> DownloadScoresAsync(ScoresNumber num, string scoresName, int zoneId, CancellationToken cancellationToken = default)
    {
        var url = GetScoresUrl(num, scoresName, zoneId);
        var response = await Client.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public virtual async Task<DateTimeOffset> FetchScoresDateTimeAsync(string scoresName, ScoresNumber num, int zoneId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        var url = GetScoresUrl(num, scoresName, zoneId);

        using var request = new HttpRequestMessage(HttpMethod.Head, url);

        if (lastModified.HasValue)
        {
            request.Headers.IfModifiedSince = lastModified;
        }

        using var response = await Client.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            return response.Content.Headers.LastModified ?? lastModified ?? throw new Exception("Last modified is null");
        }

        response.EnsureSuccessStatusCode();

        return FixDateTime(response.Content.Headers.LastModified ?? throw new Exception("Last modified is null"));
    }

    internal async Task<ScoresInfo?> FetchLatestScoresInfoAsync(string scoresName, string zone, bool parallel = false, bool scores7 = true, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(zone, out int zoneId))
        {
            return await FetchLatestScoresInfoAsync(scoresName, zoneId, parallel, scores7, cancellationToken);
        }

        return null;
    }

    internal async Task<ScoresInfo> FetchLatestScoresInfoAsync(string scoresName, int zoneId, bool parallel = false, bool scores7 = true, CancellationToken cancellationToken = default)
    {
        var responses = new Dictionary<ScoresNumber, Task<HttpResponseMessage>>();

        foreach (var num in Enum.GetValues<ScoresNumber>())
        {
            if (!scores7 && num == ScoresNumber.Scores7)
            {
                continue; // skip Scores7 if not needed
            }

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
                return new(FixDateTime(latestModified), num);
            }

            response.Dispose();
        }

        throw new Exception("Date time did not match");
    }

    public static string GetScoresUrl(ScoresNumber num, string scoresName, int zoneId)
    {
        return $"http://scores.trackmaniaforever.com/scores{(int)num}/{scoresName}/{scoresName}{zoneId}.gz";
    }

    public static string GetGeneralScoresUrl(ScoresNumber num, int zoneId)
    {
        return GetScoresUrl(num, GeneralScoresName, zoneId);
    }

    public static string GetLadderScoresUrl(ScoresNumber num, int zoneId)
    {
        return GetScoresUrl(num, LadderScoresName, zoneId);
    }

    private static DateTimeOffset FixDateTime(DateTimeOffset dateTime)
    {
        // still not done, because its too confusing to fix
        // the files have last modified date at 2:40 in CEST and 3:40 in CET
        return dateTime;
    }
}
