using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using TmScores;

namespace ManiaAPI.Xml.TMUF;

public interface IMasterServerTMUF : IMasterServer
{
    Task<LeagueRankings> GetLadderLeagueRankingsAsync(string league = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<LeagueRankings>> GetLadderLeagueRankingsResponseAsync(string league = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<PlayerRankings> GetLadderPlayerRankingsAsync(string league = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<PlayerRankings>> GetLadderPlayerRankingsResponseAsync(string league = "World", int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<PlayerAchievements> GetPlayerAchievementsAsync(string login, int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<PlayerAchievements>> GetPlayerAchievementsResponseAsync(string login, int page = 0, int count = 10, CancellationToken cancellationToken = default);
    Task<CampaignScoresInfo> GetCampaignScoresAsync(string campaignName, IEnumerable<string> leagues, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<CampaignScoresInfo>> GetCampaignScoresResponseAsync(string campaignName, IEnumerable<string> leagues, CancellationToken cancellationToken = default);
    Task<CampaignScoresLeague?> GetCampaignScoresAsync(string campaignName, string league = "World", CancellationToken cancellationToken = default);
    Task<MasterServerResponse<CampaignScoresInfo>> GetCampaignScoresResponseAsync(string campaignName, string league = "World", CancellationToken cancellationToken = default);

    Task<CampaignScores> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, int leagueId, CancellationToken cancellationToken = default);
    Task<CampaignScores?> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, string league, CancellationToken cancellationToken = default);
    Task<GeneralScores> DownloadGeneralScoresAsync(ScoresNumber num, int leagueId, CancellationToken cancellationToken = default);
    Task<GeneralScores?> DownloadGeneralScoresAsync(ScoresNumber num, string league, CancellationToken cancellationToken = default);
    Task<LadderScores> DownloadLadderScoresAsync(ScoresNumber num, int leagueId, CancellationToken cancellationToken = default);
    Task<LadderScores?> DownloadLadderScoresAsync(ScoresNumber num, string league, CancellationToken cancellationToken = default);
    Task<CampaignScores> DownloadLatestCampaignScoresAsync(string campaignName, int leagueId, CancellationToken cancellationToken = default);
    Task<CampaignScores?> DownloadLatestCampaignScoresAsync(string campaignName, string league, CancellationToken cancellationToken = default);
    Task<GeneralScores> DownloadLatestGeneralScoresAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<GeneralScores?> DownloadLatestGeneralScoresAsync(string league, CancellationToken cancellationToken = default);
    Task<LadderScores> DownloadLatestLadderScoresAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<LadderScores?> DownloadLatestLadderScoresAsync(string league, CancellationToken cancellationToken = default);
    Task<Stream> DownloadScoresAsync(ScoresNumber num, string scoresName, int leagueId, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, string league, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchGeneralScoresDateTimeAsync(ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> FetchGeneralScoresDateTimeAsync(ScoresNumber num, string league, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchLadderScoresDateTimeAsync(ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> FetchLadderScoresDateTimeAsync(ScoresNumber num, string league, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<DateTimeOffset> FetchScoresDateTimeAsync(string scoresName, ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestCampaignScoresInfoAsync(string campaignName, int leagueId, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestCampaignScoresInfoAsync(string campaignName, string league, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestGeneralScoresInfoAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestGeneralScoresInfoAsync(string league, CancellationToken cancellationToken = default);
    Task<ScoresInfo> FetchLatestLadderScoresInfoAsync(int leagueId, CancellationToken cancellationToken = default);
    Task<ScoresInfo?> FetchLatestLadderScoresInfoAsync(string league, CancellationToken cancellationToken = default);

    static abstract string GetGeneralScoresUrl(ScoresNumber num, int leagueId);
    static abstract string GetLadderScoresUrl(ScoresNumber num, int leagueId);
    static abstract string GetScoresUrl(ScoresNumber num, string scoresName, int leagueId);
}

public class MasterServerTMUF : MasterServer, IMasterServerTMUF
{
    public const string DefaultUnitedUrl = "http://game.trackmaniaforever.com/online_game/request.php";
    public const string DefaultNationsUrl = "http://game2.trackmaniaforever.com/online_game/request.php";

    private const string GeneralScoresName = "General";
    private const string LadderScoresName = "Multi";

    protected override string GameXml => "<name>TmForever</name><version>2.11.25</version>";

    public MasterServerTMUF(string url = DefaultUnitedUrl) : base(new Uri(url)) { }

    public MasterServerTMUF(MasterServerType type) : this(type switch
    {
        MasterServerType.United => DefaultUnitedUrl,
        MasterServerType.Nations => DefaultNationsUrl,
        _ => throw new ArgumentException("Invalid master server type", nameof(type))
    }) { }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMUF"/> using any <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerTMUF(HttpClient client) : base(client) { }

    public virtual async Task<MasterServerResponse<PlayerRankings>> GetLadderPlayerRankingsResponseAsync(
        string league = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetRankingsNew";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, @$"
            <t>0</t>
            <st>g</st>
            <f>{league}</f>
            <b>0</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
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
        string league = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return (await GetLadderPlayerRankingsResponseAsync(league, page, count, cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<LeagueRankings>> GetLadderLeagueRankingsResponseAsync(
        string league = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetRankingsNew";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, @$"
            <t>1</t>
            <st>g</st>
            <f>{league}</f>
            <b>0</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
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
        string league = "World",
        int page = 0,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return (await GetLadderLeagueRankingsResponseAsync(league, page, count, cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<PlayerAchievements>> GetPlayerAchievementsResponseAsync(string login, int page = 0, int count = 10, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetRankingsNew";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, @$"
            <t>4</t>
            <st>-1||-1</st>
            <f>{login}</f>
            <b>1</b>
            <p>{page}</p>
            <c>{count}</c>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
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

    public virtual async Task<MasterServerResponse<CampaignScoresInfo>> GetCampaignScoresResponseAsync(
        string campaignName,
        IEnumerable<string> leagues,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetCampaignScores";
        var leaguesXml = new StringBuilder();
        var i = 0;
        foreach (var league in leagues)
        {
            leaguesXml.Append($"<f{i}>{league}</f{i}>");
            i++;
        }
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, $"""
                <n>{campaignName}</n>
                {leaguesXml}
                <s>0000:00:00:00:00:00</s>
                <t>2</t>
            """, cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
        {
            var campaignNameResult = string.Empty;
            var campaigns = ImmutableList.CreateBuilder<CampaignScoresLeague>();

            var pendingleague = string.Empty;
            var pendingTimestamp = DateTimeOffset.MinValue;
            var pendingType = 0;
            var hasPendingDescriptor = false;

            while (xml.TryReadStartElement(out var element))
            {
                switch (element)
                {
                    case "a":
                        campaignNameResult = xml.ReadContentAsString();
                        break;
                    case "d":
                        pendingleague = string.Empty;
                        pendingTimestamp = DateTimeOffset.MinValue;
                        pendingType = 0;

                        while (xml.TryReadStartElement(out var dElement))
                        {
                            switch (dElement)
                            {
                                case "f":
                                    pendingleague = xml.ReadContentAsString();
                                    break;
                                case "u":
                                    pendingTimestamp = DateTimeOffset.ParseExact(xml.ReadContent(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                                    break;
                                case "t":
                                    pendingType = int.Parse(xml.ReadContent());
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        hasPendingDescriptor = true;
                        break;
                    case "s":
                        var filePath = string.Empty;
                        var url = string.Empty;

                        while (xml.TryReadStartElement(out var sElement))
                        {
                            switch (sElement)
                            {
                                case "f":
                                    filePath = xml.ReadContentAsString();
                                    break;
                                case "u":
                                    url = xml.ReadContentAsString();
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        if (hasPendingDescriptor)
                        {
                            campaigns.Add(new CampaignScoresLeague(pendingleague, pendingTimestamp, pendingType, filePath, url));
                            hasPendingDescriptor = false;
                        }
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            return new CampaignScoresInfo(campaignNameResult, campaigns.ToImmutable());
        });
    }

    public virtual async Task<MasterServerResponse<CampaignScoresInfo>> GetCampaignScoresResponseAsync(
        string campaignName,
        string league = "World",
        CancellationToken cancellationToken = default)
    {
        return await GetCampaignScoresResponseAsync(campaignName, [league], cancellationToken);
    }

    public async Task<CampaignScoresInfo> GetCampaignScoresAsync(
        string campaignName,
        IEnumerable<string> leagues,
        CancellationToken cancellationToken = default)
    {
        return (await GetCampaignScoresResponseAsync(campaignName, leagues, cancellationToken)).Result;
    }

    public async Task<CampaignScoresLeague?> GetCampaignScoresAsync(
        string campaignName,
        string league = "World",
        CancellationToken cancellationToken = default)
    {
        return (await GetCampaignScoresResponseAsync(campaignName, league, cancellationToken)).Result
            .Leagues
            .FirstOrDefault(x => x.Name == league);
    }

    public virtual async Task<GeneralScores?> DownloadLatestGeneralScoresAsync(string league, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await DownloadLatestGeneralScoresAsync(leagueId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<GeneralScores> DownloadLatestGeneralScoresAsync(int leagueId, CancellationToken cancellationToken = default)
    {
        var scoresInfo = await FetchLatestGeneralScoresInfoAsync(leagueId, cancellationToken);

        return await DownloadGeneralScoresAsync(scoresInfo.Number, leagueId, cancellationToken);
    }

    public virtual async Task<CampaignScores?> DownloadLatestCampaignScoresAsync(string campaignName, string league, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await DownloadLatestCampaignScoresAsync(campaignName, leagueId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<CampaignScores> DownloadLatestCampaignScoresAsync(string campaignName, int leagueId, CancellationToken cancellationToken = default)
    {
        var scoresInfo = await FetchLatestCampaignScoresInfoAsync(campaignName, leagueId, cancellationToken);

        return await DownloadCampaignScoresAsync(campaignName, scoresInfo.Number, leagueId, cancellationToken);
    }

    public virtual async Task<LadderScores?> DownloadLatestLadderScoresAsync(string league, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await DownloadLatestLadderScoresAsync(leagueId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<LadderScores> DownloadLatestLadderScoresAsync(int leagueId, CancellationToken cancellationToken = default)
    {
        var scoresInfo = await FetchLatestLadderScoresInfoAsync(leagueId, cancellationToken);

        return await DownloadLadderScoresAsync(scoresInfo.Number, leagueId, cancellationToken);
    }

    public virtual async Task<GeneralScores?> DownloadGeneralScoresAsync(ScoresNumber num, string league, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await DownloadGeneralScoresAsync(num, leagueId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<GeneralScores> DownloadGeneralScoresAsync(ScoresNumber num, int leagueId, CancellationToken cancellationToken = default)
    {
        using var stream = await DownloadScoresAsync(num, GeneralScoresName, leagueId, cancellationToken);
        return GeneralScores.Deserialize(stream);
    }

    public virtual async Task<CampaignScores?> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, string league, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await DownloadCampaignScoresAsync(campaignName, num, leagueId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<CampaignScores> DownloadCampaignScoresAsync(string campaignName, ScoresNumber num, int leagueId, CancellationToken cancellationToken = default)
    {
        using var stream = await DownloadScoresAsync(num, campaignName, leagueId, cancellationToken);
        return CampaignScores.Deserialize(stream);
    }

    public virtual async Task<LadderScores> DownloadLadderScoresAsync(ScoresNumber num, int leagueId, CancellationToken cancellationToken = default)
    {
        using var stream = await DownloadScoresAsync(num, LadderScoresName, leagueId, cancellationToken);
        return LadderScores.Deserialize(stream);
    }

    public virtual async Task<LadderScores?> DownloadLadderScoresAsync(ScoresNumber num, string league, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await DownloadLadderScoresAsync(num, leagueId, cancellationToken);
        }

        return null;
    }

    public virtual async Task<ScoresInfo?> FetchLatestGeneralScoresInfoAsync(string league, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, league, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestGeneralScoresInfoAsync(int leagueId, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(GeneralScoresName, leagueId, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo?> FetchLatestLadderScoresInfoAsync(string league, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(LadderScoresName, league, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestLadderScoresInfoAsync(int leagueId, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(LadderScoresName, leagueId, scores7: false, cancellationToken);
    }

    public virtual async Task<ScoresInfo?> FetchLatestCampaignScoresInfoAsync(string campaignName, string league, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, league, scores7: true, cancellationToken);
    }

    public virtual async Task<ScoresInfo> FetchLatestCampaignScoresInfoAsync(string campaignName, int leagueId, CancellationToken cancellationToken = default)
    {
        return await FetchLatestScoresInfoAsync(campaignName, leagueId, scores7: true, cancellationToken);
    }

    public async Task<DateTimeOffset> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        return await FetchScoresDateTimeAsync(campaignName, num, leagueId, lastModified, cancellationToken);
    }

    public async Task<DateTimeOffset?> FetchCampaignScoresDateTimeAsync(string campaignName, ScoresNumber num, string league, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await FetchCampaignScoresDateTimeAsync(campaignName, num, leagueId, lastModified, cancellationToken);
        }

        return null;
    }

    public async Task<DateTimeOffset> FetchGeneralScoresDateTimeAsync(ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        return await FetchScoresDateTimeAsync(GeneralScoresName, num, leagueId, lastModified, cancellationToken);
    }

    public async Task<DateTimeOffset?> FetchGeneralScoresDateTimeAsync(ScoresNumber num, string league, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await FetchGeneralScoresDateTimeAsync(num, leagueId, lastModified, cancellationToken);
        }

        return null;
    }

    public async Task<DateTimeOffset> FetchLadderScoresDateTimeAsync(ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        return await FetchScoresDateTimeAsync(LadderScoresName, num, leagueId, lastModified, cancellationToken);
    }

    public async Task<DateTimeOffset?> FetchLadderScoresDateTimeAsync(ScoresNumber num, string league, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await FetchLadderScoresDateTimeAsync(num, leagueId, lastModified, cancellationToken);
        }

        return null;
    }

    public virtual async Task<Stream> DownloadScoresAsync(ScoresNumber num, string scoresName, int leagueId, CancellationToken cancellationToken = default)
    {
        var url = GetScoresUrl(num, scoresName, leagueId);
        var response = await Client.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public virtual async Task<DateTimeOffset> FetchScoresDateTimeAsync(string scoresName, ScoresNumber num, int leagueId, DateTimeOffset? lastModified = null, CancellationToken cancellationToken = default)
    {
        var url = GetScoresUrl(num, scoresName, leagueId);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

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

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var timestamp = GetGzipTimestamp(stream) ?? response.Content.Headers.LastModified;

        return FixDateTime(timestamp ?? response.Content.Headers.LastModified ?? throw new Exception("Last modified is null"));
    }

    internal async Task<ScoresInfo?> FetchLatestScoresInfoAsync(string scoresName, string league, bool scores7 = true, CancellationToken cancellationToken = default)
    {
        if (League.IdsWithDataInTMUF.TryGetValue(league, out int leagueId))
        {
            return await FetchLatestScoresInfoAsync(scoresName, leagueId, scores7, cancellationToken);
        }

        return null;
    }

    internal async Task<ScoresInfo> FetchLatestScoresInfoAsync(string scoresName, int leagueId, bool scores7 = true, CancellationToken cancellationToken = default)
    {
        var responses = new Dictionary<ScoresNumber, Task<HttpResponseMessage>>();

        foreach (var num in Enum.GetValues<ScoresNumber>())
        {
            if (!scores7 && num == ScoresNumber.Scores7)
            {
                continue; // skip Scores7 if not needed
            }

            var url = GetScoresUrl(num, scoresName, leagueId);
            var task = Client.GetAsync(url, cancellationToken);

            responses.Add(num, task);
        }

        var latestScoresNum = default(ScoresNumber?);
        var latestTimestamp = default(DateTimeOffset?);

        foreach (var (num, responseTask) in responses)
        {
            var response = await responseTask;

            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var timestamp = GetGzipTimestamp(stream) ?? response.Content.Headers.LastModified;

            if (latestTimestamp is null || timestamp > latestTimestamp)
            {
                latestScoresNum = num;
                latestTimestamp = timestamp;
            }
        }

        if (responses.All(x => !x.Value.Result.IsSuccessStatusCode))
        {
            throw new Exception("No successful responses");
        }

        return new ScoresInfo(FixDateTime(latestTimestamp ?? throw new Exception("Last modified is null")), latestScoresNum ?? throw new Exception("Latest scores number is null"));
    }

    public static string GetScoresUrl(ScoresNumber num, string scoresName, int leagueId)
    {
        return $"http://scores.trackmaniaforever.com/scores{(int)num}/{scoresName}/{scoresName}{leagueId}.gz";
    }

    public static string GetGeneralScoresUrl(ScoresNumber num, int leagueId)
    {
        return GetScoresUrl(num, GeneralScoresName, leagueId);
    }

    public static string GetLadderScoresUrl(ScoresNumber num, int leagueId)
    {
        return GetScoresUrl(num, LadderScoresName, leagueId);
    }

    private static DateTimeOffset FixDateTime(DateTimeOffset dateTime)
    {
        // still not done, because its too confusing to fix
        // the files have last modified date at 2:40 in CEST and 3:40 in CET
        return dateTime;
    }

    private static DateTimeOffset? GetGzipTimestamp(Stream stream)
    {
        if (!stream.CanSeek)
        {
            return null;
        }

        Span<byte> buffer = stackalloc byte[10];
        stream.ReadExactly(buffer);

        if (buffer[0] != 0x1F || buffer[1] != 0x8B)
        {
            throw new InvalidDataException("Invalid GZip header while reading timestamp.");
        }

        var mtime = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4, 4));

        if (mtime == 0)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(mtime);
    }
}
