using MinimalXmlReader;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TmEssentials;

namespace ManiaAPI.XmlRpc.MP4;

public interface IMasterServerMP4 : IMasterServer
{
    Task ValidateAsync(InitServerMP4 initServer, CancellationToken cancellationToken = default);
    Task ValidateAsync(CancellationToken cancellationToken = default);

    Task<MasterServerResponse<IReadOnlyCollection<LeaderboardItem<uint>>>> GetCampaignLeaderBoardResponseAsync(string titleId, string? campaignId = null, int count = 10, int offset = 0, string zone = "World", CampaignLeaderboardType type = CampaignLeaderboardType.SkillPoint, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LeaderboardItem<uint>>> GetCampaignLeaderBoardAsync(string titleId, string? campaignId = null, int count = 10, int offset = 0, string zone = "World", CampaignLeaderboardType type = CampaignLeaderboardType.SkillPoint, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<IReadOnlyCollection<LeaderboardItem<TimeInt32>>>> GetMapLeaderBoardResponseAsync(string titleId, string mapUid, int count = 10, int offset = 0, string zone = "World", string context = "", CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LeaderboardItem<TimeInt32>>> GetMapLeaderBoardAsync(string titleId, string mapUid, int count = 10, int offset = 0, string zone = "World", string context = "", CancellationToken cancellationToken = default);
    Task<MasterServerResponse<IReadOnlyCollection<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(string titleId, IEnumerable<CampaignSummaryRequest> summaries, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<IReadOnlyCollection<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(string titleId, params IEnumerable<CampaignSummaryRequest> summaries);
    Task<IReadOnlyCollection<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(string titleId, IEnumerable<CampaignSummaryRequest> summaries, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(string titleId, params IEnumerable<CampaignSummaryRequest> summaries);
    Task<MasterServerResponse<IReadOnlyCollection<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(string titleId, IEnumerable<MapSummaryRequest> summaries, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<IReadOnlyCollection<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(string titleId, params IEnumerable<MapSummaryRequest> summaries);
    Task<IReadOnlyCollection<MapSummary>> GetMapLeaderBoardSummariesAsync(string titleId, IEnumerable<MapSummaryRequest> summaries, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MapSummary>> GetMapLeaderBoardSummariesAsync(string titleId, params IEnumerable<MapSummaryRequest> summaries);
}

public class MasterServerMP4 : MasterServer, IMasterServerMP4
{
    public const string DefaultAddress = "http://relay02.v04.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlRpcHelperMP4.GameXml;

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
    /// <param name="client">HTTP client.</param>
    public MasterServerMP4(HttpClient client) : base(client)
    {
    }

    protected string GetGameXml(string titleId) => @$"{GameXml}
<title>{titleId}</title>";

    public virtual async Task ValidateAsync(InitServerMP4 initServer, CancellationToken cancellationToken = default)
    {
        try
        {
            await GetApplicationConfigAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            var waitingParams = await initServer.GetWaitingParamsAsync(cancellationToken);
            Client.BaseAddress = waitingParams.MasterServers.First().GetUri();
        }
    }

    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        await ValidateAsync(new InitServerMP4(), cancellationToken);
    }

    private async Task GetApplicationConfigAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetApplicationConfig";
        _ = await XmlRpcHelper.SendAsync(Client, GameXml, RequestName, string.Empty, cancellationToken);
    }

    public virtual async Task<MasterServerResponse<IReadOnlyCollection<LeaderboardItem<uint>>>> GetCampaignLeaderBoardResponseAsync(
        string titleId,
        string? campaignId = null,
        int count = 10,
        int offset = 0,
        string zone = "World",
        CampaignLeaderboardType type = CampaignLeaderboardType.SkillPoint,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetCampaignLeaderBoard";
        var response = await XmlRpcHelper.SendAsync(Client, GetGameXml(titleId), RequestName, @$"
            <f>{offset}</f>
            <n>{count}</n>
            <c>{campaignId ?? titleId}</c>
            <m></m>
            <t></t>
            <z>{zone}</z>
            <s>{type}</s>", cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, ReadLeaderboardItems<uint>);
    }

    public async Task<IReadOnlyCollection<LeaderboardItem<uint>>> GetCampaignLeaderBoardAsync(
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

    public virtual async Task<MasterServerResponse<IReadOnlyCollection<LeaderboardItem<TimeInt32>>>> GetMapLeaderBoardResponseAsync(
        string titleId,
        string mapUid,
        int count = 10,
        int offset = 0,
        string zone = "World",
        string context = "",
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMapLeaderBoard";
        var response = await XmlRpcHelper.SendAsync(Client, GetGameXml(titleId), RequestName, @$"
            <m>{mapUid}</m>
            <n>{count}</n>
            <f>{offset}</f>
            <z>{zone}</z>
            <c></c>
            <t>{context}</t>
            <s>MapRecord</s>", cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, ReadLeaderboardItems<TimeInt32>);
    }

    public async Task<IReadOnlyCollection<LeaderboardItem<TimeInt32>>> GetMapLeaderBoardAsync(
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

    public virtual async Task<MasterServerResponse<IReadOnlyCollection<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(
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

        var response = await XmlRpcHelper.SendAsync(Client, GetGameXml(titleId), RequestName, sb.ToString(), cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var summaries = new List<CampaignSummary>();

            while (xml.TryReadStartElement("s"))
            {
                var campaignId = string.Empty;
                var zone = string.Empty;
                var type = CampaignLeaderboardType.SkillPoint;
                var timestamp = DateTimeOffset.UtcNow;
                var count = 0;
                var skillpoints = Array.Empty<RecordUnit<uint>>();
                var highScores = Array.Empty<LeaderboardItem<uint>>();

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

            return (IReadOnlyCollection<CampaignSummary>)summaries;
        });
    }

    public async Task<MasterServerResponse<IReadOnlyCollection<CampaignSummary>>> GetCampaignLeaderBoardSummariesResponseAsync(
        string titleId,
        params IEnumerable<CampaignSummaryRequest> summaries)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync(titleId, summaries, default);
    }

    public async Task<IReadOnlyCollection<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(
        string titleId,
        IEnumerable<CampaignSummaryRequest> summaries,
        CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardSummariesResponseAsync(titleId, summaries, cancellationToken)).Result;
    }

    public async Task<IReadOnlyCollection<CampaignSummary>> GetCampaignLeaderBoardSummariesAsync(
        string titleId,
        params IEnumerable<CampaignSummaryRequest> summaries)
    {
        return await GetCampaignLeaderBoardSummariesAsync(titleId, summaries, default);
    }

    public virtual async Task<MasterServerResponse<IReadOnlyCollection<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(
        string titleId,
        IEnumerable<MapSummaryRequest> summaries,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMapLeaderBoardSummaries";

        var sb = new StringBuilder("\n<b>1</b>");
        var i = 1;
        foreach (var summary in summaries)
        {
            sb.Append($"\n<c{i}></c{i}>");
            sb.Append($"\n<m{i}>{summary.MapUid}</m{i}>");
            sb.Append($"\n<t{i}></t{i}>");
            sb.Append($"\n<z{i}>{summary.Zone}</z{i}>");
            sb.Append($"\n<s{i}>{summary.Type}</s{i}>");
            i++;
        }

        var response = await XmlRpcHelper.SendAsync(Client, GetGameXml(titleId), RequestName, sb.ToString(), cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var summaries = new List<MapSummary>();

            while (xml.TryReadStartElement("s"))
            {
                var mapUid = string.Empty;
                var zone = string.Empty;
                var type = MapLeaderboardType.MapRecord;
                var timestamp = DateTimeOffset.UtcNow;
                var count = 0;
                var skillpoints = Array.Empty<RecordUnit<TimeInt32>>();
                var highScores = Array.Empty<LeaderboardItem<TimeInt32>>();

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
                            skillpoints = ReadAllLeaderboardRecords<TimeInt32>(ref xml);
                            break;
                        case "t":
                            highScores = ReadLeaderboardRecords<TimeInt32>(ref xml);
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                summaries.Add(new MapSummary(mapUid, zone, type, timestamp, count, skillpoints, highScores));

                _ = xml.SkipEndElement(); // s
            }

            return (IReadOnlyCollection<MapSummary>)summaries;
        });
    }

    public async Task<MasterServerResponse<IReadOnlyCollection<MapSummary>>> GetMapLeaderBoardSummariesResponseAsync(
        string titleId,
        params IEnumerable<MapSummaryRequest> summaries)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(titleId, summaries, default);
    }

    public async Task<IReadOnlyCollection<MapSummary>> GetMapLeaderBoardSummariesAsync(
        string titleId,
        IEnumerable<MapSummaryRequest> summaries,
        CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardSummariesResponseAsync(titleId, summaries, cancellationToken)).Result;
    }

    public async Task<IReadOnlyCollection<MapSummary>> GetMapLeaderBoardSummariesAsync(
        string titleId,
        params IEnumerable<MapSummaryRequest> summaries)
    {
        return await GetMapLeaderBoardSummariesAsync(titleId, summaries, default);
    }

    private static IReadOnlyCollection<LeaderboardItem<T>> ReadLeaderboardItems<T>(ref MiniXmlReader xml) where T : struct
    {
        var items = new List<LeaderboardItem<T>>();

        while (xml.TryReadStartElement("i"))
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

            items.Add(new LeaderboardItem<T>(rank, login, nickname, scoreValue, fileName, downloadUrl));

            _ = xml.SkipEndElement(); // i
        }

        return items;
    }

    private static RecordUnit<T>[] ReadAllLeaderboardRecords<T>(ref MiniXmlReader xml) where T : struct
    {
        var scoreData = Convert.FromBase64String(xml.ReadContentAsString());

        if (scoreData.Length <= 4)
        {
            return [];
        }

        var uniqueScoreDataCount = BitConverter.ToInt32(scoreData, 0);

        return MemoryMarshal.Cast<byte, RecordUnit<T>>(scoreData.AsSpan().Slice(4)).ToArray();
    }

    private static LeaderboardItem<T>[] ReadLeaderboardRecords<T>(ref MiniXmlReader xml) where T : struct
    {
        using var ms = new MemoryStream(Convert.FromBase64String(xml.ReadContentAsString()));
        using var r = new GbxBasedReader(ms, leaveOpen: false);

        var records = new LeaderboardItem<T>[r.ReadUInt32()];

        for (int i = 0; i < records.Length; i++)
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

        return records;
    }
}
