using MinimalXmlReader;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TmEssentials;

namespace ManiaAPI.Xml.MP4;

public interface IMasterServerMP4 : IMasterServer
{
    /// <summary>
    /// Gets the campaign leaderboard. If <paramref name="campaignId"/> is <see langword="null"/>, it will use the <paramref name="titleId"/> instead.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="campaignId">Campaign ID, usually the same as the title ID for single campaign titles.</param>
    /// <param name="count">Number of entries to retrieve.</param>
    /// <param name="offset">Offset to start retrieving entries from.</param>
    /// <param name="zone">Zone to retrieve the leaderboard from.</param>
    /// <param name="type">Type of leaderboard to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the leaderboard entries.</returns>
    Task<MasterServerResponse<ImmutableList<LeaderboardItem<uint>>>> GetCampaignLeaderBoardResponseAsync(string titleId, string? campaignId = null, int count = 10, int offset = 0, string zone = "World", CampaignLeaderboardType type = CampaignLeaderboardType.SkillPoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the campaign leaderboard. If <paramref name="campaignId"/> is <see langword="null"/>, it will use the <paramref name="titleId"/> instead.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="campaignId">Campaign ID, usually the same as the title ID for single campaign titles.</param>
    /// <param name="count">Number of entries to retrieve.</param>
    /// <param name="offset">Offset to start retrieving entries from.</param>
    /// <param name="zone">Zone to retrieve the leaderboard from.</param>
    /// <param name="type">Type of leaderboard to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the leaderboard entries.</returns>
    Task<ImmutableList<LeaderboardItem<uint>>> GetCampaignLeaderBoardAsync(string titleId, string? campaignId = null, int count = 10, int offset = 0, string zone = "World", CampaignLeaderboardType type = CampaignLeaderboardType.SkillPoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the map leaderboard.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="mapUid">The unique ID (MapUid) of the map.</param>
    /// <param name="count">Number of entries to retrieve.</param>
    /// <param name="offset">Offset to start retrieving entries from.</param>
    /// <param name="zone">Zone to retrieve the leaderboard from.</param>
    /// <param name="context">Score context, usually empty string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the leaderboard entries.</returns>
    Task<MasterServerResponse<ImmutableList<LeaderboardItem<TimeInt32>>>> GetMapLeaderBoardResponseAsync(string titleId, string mapUid, int count = 10, int offset = 0, string zone = "World", string context = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the map leaderboard.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="mapUid">The unique ID (MapUid) of the map.</param>
    /// <param name="count">Number of entries to retrieve.</param>
    /// <param name="offset">Offset to start retrieving entries from.</param>
    /// <param name="zone">Zone to retrieve the leaderboard from.</param>
    /// <param name="context">Score context, usually empty string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the leaderboard entries.</returns>
    Task<ImmutableList<LeaderboardItem<TimeInt32>>> GetMapLeaderBoardAsync(string titleId, string mapUid, int count = 10, int offset = 0, string zone = "World", string context = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple campaign leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of campaign requests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the campaign summaries.</returns>
    Task<MasterServerResponse<ImmutableList<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(string titleId, IEnumerable<CampaignSummaryRequest> summaries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple campaign leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of campaign requests.</param>
    /// <returns>A task with the result containing the campaign summaries.</returns>
    Task<MasterServerResponse<ImmutableList<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(string titleId, params IEnumerable<CampaignSummaryRequest> summaries);

    /// <summary>
    /// Gets multiple campaign leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of campaign requests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the campaign summaries.</returns>
    Task<ImmutableList<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(string titleId, IEnumerable<CampaignSummaryRequest> summaries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple campaign leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of campaign requests.</param>
    /// <returns>A task with the result containing the campaign summaries.</returns>
    Task<ImmutableList<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(string titleId, params IEnumerable<CampaignSummaryRequest> summaries);

    /// <summary>
    /// Gets multiple map leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of map requests.</param>
    /// <param name="isBinary">Whether to use binary format for scores (smaller payload) or XML format (larger payload). Default is <see langword="true"/> as it is objectively better, but you can set this to <see langword="false"/> to skip a cache once.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the map summaries.</returns>
    Task<MasterServerResponse<ImmutableList<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(string titleId, IEnumerable<MapSummaryRequest> summaries, bool isBinary = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple map leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of map requests.</param>
    /// <returns>A task with the result containing the map summaries.</returns>
    Task<MasterServerResponse<ImmutableList<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(string titleId, params IEnumerable<MapSummaryRequest> summaries);

    /// <summary>
    /// Gets multiple map leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of map requests.</param>
    /// <param name="isBinary">Whether to use binary format for scores (smaller payload) or XML format (larger payload). Default is <see langword="true"/> as it is objectively better, but you can set this to <see langword="false"/> to skip a cache once.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the map summaries.</returns>
    Task<ImmutableList<MapSummary>> GetMapLeaderBoardSummariesAsync(string titleId, IEnumerable<MapSummaryRequest> summaries, bool isBinary = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple map leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="summaries">The list of map requests.</param>
    /// <returns>A task with the result containing the map summaries.</returns>
    Task<ImmutableList<MapSummary>> GetMapLeaderBoardSummariesAsync(string titleId, params IEnumerable<MapSummaryRequest> summaries);
}

public class MasterServerMP4 : MasterServer, IMasterServerMP4
{
    public const string DefaultAddress = "https://relay02.v04.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlHelperMP4.GameXml;

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP4"/> using the expected master server address (relay02). In case it's offline, you need to check <see cref="InitServerMP4"/>.
    /// </summary>
    public MasterServerMP4() : base(new Uri(DefaultAddress))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP4"/> using a <see cref="MasterServerInfo"/> object. Be careful to use the correct object given from the correct init server.
    /// </summary>
    /// <param name="info">Info about the master server, usually given from <see cref="InitServerMP4"/>.</param>
    public MasterServerMP4(MasterServerInfo info) : base(info.GetUri())
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP4"/> using any <see cref="HttpClient"/>. You need to set the base address yourself.
    /// </summary>
    /// <param name="uri">URI of the master server.</param>
    /// <param name="client">HTTP client.</param>
    public MasterServerMP4(Uri uri, HttpClient client) : base(uri, client)
    {
    }

    protected string GetGameXml(string titleId) => $"{GameXml}<title>{titleId}</title>";

    internal async Task GetApplicationConfigAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetApplicationConfig";
        _ = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml: null, RequestName, string.Empty, cancellationToken);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<LeaderboardItem<uint>>>> GetCampaignLeaderBoardResponseAsync(
        string titleId,
        string? campaignId = null,
        int count = 10,
        int offset = 0,
        string zone = "World",
        CampaignLeaderboardType type = CampaignLeaderboardType.SkillPoint,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetCampaignLeaderBoard";
        var response = await XmlHelper.SendAsync(Client, ServerUri, GetGameXml(titleId), authorXml: null, RequestName, @$"
            <f>{offset}</f>
            <n>{count}</n>
            <c>{campaignId ?? titleId}</c>
            <m></m>
            <t></t>
            <z>{zone}</z>
            <s>{type}</s>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, ReadLeaderboardItems<uint>);
    }

    public async Task<ImmutableList<LeaderboardItem<uint>>> GetCampaignLeaderBoardAsync(
        string titleId,
        string? campaignId = null,
        int count = 10,
        int offset = 0,
        string zone = "World",
        CampaignLeaderboardType type = CampaignLeaderboardType.SkillPoint,
        CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardResponseAsync(titleId, campaignId, count, offset, zone, type, cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<ImmutableList<LeaderboardItem<TimeInt32>>>> GetMapLeaderBoardResponseAsync(
        string titleId,
        string mapUid,
        int count = 10,
        int offset = 0,
        string zone = "World",
        string context = "",
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMapLeaderBoard";
        var response = await XmlHelper.SendAsync(Client, ServerUri, GetGameXml(titleId), authorXml: null, RequestName, @$"
            <m>{mapUid}</m>
            <n>{count}</n>
            <f>{offset}</f>
            <z>{zone}</z>
            <c></c>
            <t>{context}</t>
            <s>MapRecord</s>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, ReadLeaderboardItems<TimeInt32>);
    }

    public async Task<ImmutableList<LeaderboardItem<TimeInt32>>> GetMapLeaderBoardAsync(
        string titleId,
        string mapUid,
        int count = 10,
        int offset = 0,
        string zone = "World",
        string context = "",
        CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardResponseAsync(titleId, mapUid, count, offset, zone, context, cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<ImmutableList<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(
        string titleId,
        IEnumerable<CampaignSummaryRequest> summaries,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetCampaignLeaderBoardSummaries";

        var sb = new StringBuilder("\n<b>1</b>");
        var i = 1;
        foreach (var summary in summaries)
        {
            sb.Append($"\n<c{i}>{summary.CampaignId ?? titleId}</c{i}>");
            sb.Append($"\n<m{i}></m{i}>");
            sb.Append($"\n<t{i}></t{i}>");
            sb.Append($"\n<z{i}>{summary.Zone}</z{i}>");
            sb.Append($"\n<s{i}>{summary.Type}</s{i}>");
            i++;
        }

        var response = await XmlHelper.SendAsync(Client, ServerUri, GetGameXml(titleId), authorXml: null, RequestName, sb.ToString(), cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var summaries = ImmutableList.CreateBuilder<CampaignSummary>();

            while (xml.TryReadStartElement("s"))
            {
                var campaignId = string.Empty;
                var zone = string.Empty;
                var type = CampaignLeaderboardType.SkillPoint;
                var timestamp = DateTimeOffset.UtcNow;
                var count = 0;
                var skillpoints = ImmutableArray<RecordUnit<uint>>.Empty;
                var highScores = ImmutableArray<LeaderboardItem<uint>>.Empty;

                while (xml.TryReadStartElement(out var itemElement))
                {
                    switch (itemElement)
                    {
                        case "c":
                            campaignId = xml.ReadContentAsString();
                            break;
                        case "z":
                            zone = xml.ReadContentAsString();
                            break;
                        case "s":
                            _ = Enum.TryParse(xml.ReadContentAsString(), out type);
                            break;
                        case "d":
                            timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                            break;
                        case "n":
                            count = int.Parse(xml.ReadContent());
                            break;
                        case "i":
                            skillpoints = ReadAllLeaderboardRecords<uint>(ref xml);
                            break;
                        case "t":
                            highScores = ReadLeaderboardRecords<uint>(ref xml);
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                summaries.Add(new CampaignSummary(campaignId, zone, type, timestamp, count, skillpoints, highScores));

                _ = xml.SkipEndElement(); // s
            }

            return summaries.ToImmutable();
        });
    }

    public async Task<MasterServerResponse<ImmutableList<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(
        string titleId,
        params IEnumerable<CampaignSummaryRequest> summaries)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync(titleId, summaries, cancellationToken: default);
    }

    public async Task<ImmutableList<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(
        string titleId,
        IEnumerable<CampaignSummaryRequest> summaries,
        CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardSummariesResponseAsync(titleId, summaries, cancellationToken)).Result;
    }

    public async Task<ImmutableList<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(
        string titleId,
        params IEnumerable<CampaignSummaryRequest> summaries)
    {
        return await GetCampaignLeaderBoardSummariesAsync(titleId, summaries, cancellationToken: default);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(
        string titleId,
        IEnumerable<MapSummaryRequest> summaries,
        bool isBinary = true,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMapLeaderBoardSummaries";

        var sb = new StringBuilder($"\n<b>{(isBinary ? 1 : 0)}</b>");
        var i = 1;
        foreach (var summary in summaries)
        {
            sb.Append($"\n<c{i}></c{i}>");
            sb.Append($"\n<m{i}>{summary.MapUid}</m{i}>");
            sb.Append($"\n<t{i}>{summary.Context}</t{i}>");
            sb.Append($"\n<z{i}>{summary.Zone}</z{i}>");
            sb.Append($"\n<s{i}>{summary.Type}</s{i}>");
            i++;
        }

        var response = await XmlHelper.SendAsync(Client, ServerUri, GetGameXml(titleId), authorXml: null, RequestName, sb.ToString(), cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var summaries = ImmutableList.CreateBuilder<MapSummary>();

            while (xml.TryReadStartElement("s"))
            {
                var mapUid = string.Empty;
                var zone = string.Empty;
                var type = MapLeaderboardType.MapRecord;
                var timestamp = DateTimeOffset.UtcNow;
                var count = 0;
                var skillpoints = ImmutableArray<RecordUnit<TimeInt32>>.Empty;
                var highScores = ImmutableArray<LeaderboardItem<TimeInt32>>.Empty;
                var skillpointsBuilder = ImmutableArray.CreateBuilder<RecordUnit<TimeInt32>>();
                var highScoresBuilder = ImmutableArray.CreateBuilder<LeaderboardItem<TimeInt32>>();

                while (xml.TryReadStartElement(out var itemElement))
                {
                    switch (itemElement)
                    {
                        case "m":
                            mapUid = xml.ReadContentAsString();
                            break;
                        case "z":
                            zone = xml.ReadContentAsString();
                            break;
                        case "s":
                            _ = Enum.TryParse(xml.ReadContentAsString(), out type);
                            break;
                        case "d":
                            timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                            break;
                        case "n":
                            count = int.Parse(xml.ReadContent());
                            break;
                        case "i":
                            if (isBinary)
                            {
                                skillpoints = ReadAllLeaderboardRecords<TimeInt32>(ref xml);
                            }
                            else
                            {
                                skillpointsBuilder.Add(XmlHelper.ReadRecordUnit<TimeInt32>(ref xml));
                            }
                            break;
                        case "t":
                            if (isBinary)
                            {
                                highScores = ReadLeaderboardRecords<TimeInt32>(ref xml);
                            }
                            else
                            {
                                highScoresBuilder.Add(ReadLeaderboardItem<TimeInt32>(ref xml));
                            }
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                if (!isBinary)
                {
                    skillpoints = skillpointsBuilder.ToImmutable();
                    highScores = highScoresBuilder.ToImmutable();
                }

                summaries.Add(new MapSummary(mapUid, zone, type, timestamp, count, skillpoints, highScores));

                _ = xml.SkipEndElement(); // s
            }

            return summaries.ToImmutable();
        });
    }

    public async Task<MasterServerResponse<ImmutableList<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(
        string titleId,
        params IEnumerable<MapSummaryRequest> summaries)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(titleId, summaries, isBinary: true, cancellationToken: default);
    }

    public async Task<ImmutableList<MapSummary>> GetMapLeaderBoardSummariesAsync(
        string titleId,
        IEnumerable<MapSummaryRequest> summaries,
        bool isBinary = true,
        CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardSummariesResponseAsync(titleId, summaries, isBinary, cancellationToken)).Result;
    }

    public async Task<ImmutableList<MapSummary>> GetMapLeaderBoardSummariesAsync(
        string titleId,
        params IEnumerable<MapSummaryRequest> summaries)
    {
        return await GetMapLeaderBoardSummariesAsync(titleId, summaries, isBinary: true, cancellationToken: default);
    }

    private static ImmutableList<LeaderboardItem<T>> ReadLeaderboardItems<T>(ref MiniXmlReader xml) where T : struct
    {
        var items = ImmutableList.CreateBuilder<LeaderboardItem<T>>();

        while (xml.TryReadStartElement("i"))
        {
            items.Add(ReadLeaderboardItem<T>(ref xml));

            _ = xml.SkipEndElement(); // i
        }

        return items.ToImmutable();
    }

    private static LeaderboardItem<T> ReadLeaderboardItem<T>(ref MiniXmlReader xml) where T : struct
    {
        var rank = 0;
        var login = string.Empty;
        var nickname = string.Empty;
        var score = 0u;
        var fileName = string.Empty;
        var downloadUrl = string.Empty;

        while (xml.TryReadStartElement(out var itemElement))
        {
            switch (itemElement)
            {
                case "r":
                    rank = int.Parse(xml.ReadContent());
                    break;
                case "l":
                    login = xml.ReadContentAsString();
                    break;
                case "n":
                    nickname = xml.ReadContentAsString();
                    break;
                case "s":
                    score = uint.Parse(xml.ReadContent());
                    break;
                case "f":
                    fileName = xml.ReadContentAsString();
                    break;
                case "u":
                    downloadUrl = xml.ReadContentAsString();
                    break;
                default:
                    xml.ReadContent();
                    break;
            }

            _ = xml.SkipEndElement();
        }

        ref T scoreValue = ref Unsafe.As<uint, T>(ref score);

        return new LeaderboardItem<T>(rank, login, nickname, scoreValue, fileName, downloadUrl);
    }

    private static ImmutableArray<RecordUnit<T>> ReadAllLeaderboardRecords<T>(ref MiniXmlReader xml) where T : struct
    {
        var scoreData = Convert.FromBase64String(xml.ReadContentAsString());

        if (scoreData.Length <= 4)
        {
            return [];
        }

        var uniqueScoreDataCount = BitConverter.ToInt32(scoreData, 0);

        return MemoryMarshal.Cast<byte, RecordUnit<T>>(scoreData.AsSpan().Slice(4)).ToImmutableArray();
    }

    private static ImmutableArray<LeaderboardItem<T>> ReadLeaderboardRecords<T>(ref MiniXmlReader xml) where T : struct
    {
        using var ms = new MemoryStream(Convert.FromBase64String(xml.ReadContentAsString()));
        using var r = new GbxBasedReader(ms, leaveOpen: false);

        var records = ImmutableArray.CreateBuilder<LeaderboardItem<T>>(r.ReadInt32());

        for (int i = 0; i < records.Count; i++)
        {
            var rank = r.ReadInt32();
            var score = r.ReadUInt32();
            var login = r.ReadString();
            var nickname = r.ReadString();
            var fileName = r.ReadString();
            var replayUrl = r.ReadString();

            ref T scoreValue = ref Unsafe.As<uint, T>(ref score);

            records[i] = new LeaderboardItem<T>(rank, login, nickname, scoreValue, fileName, replayUrl);
        }

        return records.ToImmutable();
    }
}
