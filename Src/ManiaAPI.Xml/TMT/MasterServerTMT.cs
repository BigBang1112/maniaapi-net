using MinimalXmlReader;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using TmEssentials;

namespace ManiaAPI.Xml.TMT;

public interface IMasterServerTMT : IMasterServer
{
    Task<MasterServerResponse<ImmutableList<SummaryZone<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<SummaryZone<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(string zone = "World", CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<SummaryZone<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(params IEnumerable<string> zones);
    Task<ImmutableList<SummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<ImmutableList<SummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", CancellationToken cancellationToken = default);
    Task<ImmutableList<SummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones);
    Task<MasterServerResponse<ImmutableList<SummaryZone<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<SummaryZone<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<SummaryZone<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, params IEnumerable<string> zones);
    Task<ImmutableList<SummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<ImmutableList<SummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default);
    Task<ImmutableList<SummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones);

    Task<MasterServerResponse<Summary<int>>> GetCampaignLeaderBoardSummaryResponseAsync(string zone = "World", CancellationToken cancellationToken = default);
    Task<Summary<int>> GetCampaignLeaderBoardSummaryAsync(string zone = "World", CancellationToken cancellationToken = default);
    Task<MasterServerResponse<Summary<TimeInt32>>> GetMapLeaderBoardSummaryResponseAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default);
    Task<Summary<TimeInt32>> GetMapLeaderBoardSummaryAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default);
}

public class MasterServerTMT : MasterServer, IMasterServerTMT
{
    protected override string GameXml => XmlHelperTMT.GameXml;

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMT"/> using a raw address. Because the address changes quite often and also because there are multiple platforms, it is recommended to use <see cref="InitServerTMT"/> to get the address first.
    /// </summary>
    /// <param name="address">The address given from <see cref="InitServerTMT"/> via <see cref="InitServer.GetWaitingParamsAsync(CancellationToken)"/>, or a custom address.</param>
    public MasterServerTMT(Uri address) : base(address)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMT"/> using a <see cref="MasterServerInfo"/> object. Be careful to use the correct object for the correct platform given from the correct init server.
    /// </summary>
    /// <param name="info">Info about the master server, usually given from <see cref="InitServerTMT"/>.</param>
    public MasterServerTMT(MasterServerInfo info) : base(info.GetUri())
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMT"/> using any <see cref="HttpClient"/>. You need to set the base address yourself.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerTMT(HttpClient client) : base(client)
    {
    }

    public virtual async Task<MasterServerResponse<ImmutableList<SummaryZone<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetCampaignLeaderBoardSummaries";

        var sb = new StringBuilder("\n<c>TMTurbo@nadeolabs</c>\n<m></m>\n<s>MultiAsyncLevel</s>");
        var i = 1;
        foreach (var zone in zones)
        {
            sb.Append($"\n<z{i}>{zone}</z{i}>");
            i++;
        }

        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, sb.ToString(), cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, ReadSummaries<int>);
    }

    public async Task<MasterServerResponse<ImmutableList<SummaryZone<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync([zone], cancellationToken);
    }

    public async Task<MasterServerResponse<ImmutableList<SummaryZone<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(params IEnumerable<string> zones)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync(zones, default);
    }

    public async Task<ImmutableList<SummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardSummariesResponseAsync(zones, cancellationToken)).Result;
    }

    public async Task<ImmutableList<SummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetCampaignLeaderBoardSummariesAsync([zone], cancellationToken);
    }

    public async Task<ImmutableList<SummaryZone<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones)
    {
        return await GetCampaignLeaderBoardSummariesAsync(zones, default);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<SummaryZone<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMapLeaderBoardSummaries";

        var sb = new StringBuilder($"\n<c>TMTurbo@nadeolabs</c>\n<m>{mapUid}</m>\n<s>MapTime</s>");
        var i = 1;
        foreach (var zone in zones)
        {
            sb.Append($"\n<z{i}>{zone}</z{i}>");
            i++;
        }

        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, sb.ToString(), cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, ReadSummaries<TimeInt32>);
    }

    public async Task<MasterServerResponse<ImmutableList<SummaryZone<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(mapUid, [zone], cancellationToken);
    }

    public async Task<MasterServerResponse<ImmutableList<SummaryZone<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, params IEnumerable<string> zones)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, default);
    }

    public async Task<ImmutableList<SummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, cancellationToken)).Result;
    }

    public async Task<ImmutableList<SummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, [zone], cancellationToken);
    }

    public async Task<ImmutableList<SummaryZone<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, zones, default);
    }

    public virtual async Task<MasterServerResponse<Summary<int>>> GetCampaignLeaderBoardSummaryResponseAsync(string zone = "World", CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetLeaderBoardSummary";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, @$"
            <t>Campaign</t>
            <c>TMTurbo@nadeolabs</c>
            <m></m>
            <s>0</s>
            <z>{zone}</z>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, ReadSummary<int>);
    }

    public async Task<Summary<int>> GetCampaignLeaderBoardSummaryAsync(string zone = "World", CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardSummaryResponseAsync(zone, cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<Summary<TimeInt32>>> GetMapLeaderBoardSummaryResponseAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetLeaderBoardSummary";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, @$"
            <t>Map</t>
            <c>TMTurbo@nadeolabs</c>
            <m>{mapUid}</m>
            <s>0</s>
            <z>{zone}</z>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, ReadSummary<TimeInt32>);
    }

    public async Task<Summary<TimeInt32>> GetMapLeaderBoardSummaryAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardSummaryResponseAsync(mapUid, zone, cancellationToken)).Result;
    }

    private static ImmutableList<SummaryZone<T>> ReadSummaries<T>(ref MiniXmlReader xml) where T : struct
    {
        var summaries = ImmutableList.CreateBuilder<SummaryZone<T>>();

        while (xml.TryReadStartElement("s"))
        {
            var zone = string.Empty;
            var timestamp = default(DateTimeOffset);
            var units = ImmutableList.CreateBuilder<RecordUnit<T>>();

            while (xml.TryReadStartElement(out var itemElement))
            {
                switch (itemElement)
                {
                    case "z":
                        zone = xml.ReadContentAsString();
                        break;
                    case "d":
                        timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                        break;
                    case "i":
                        var score = 0u;
                        var count = 0;

                        while (xml.TryReadStartElement(out var medalElement))
                        {
                            switch (medalElement)
                            {
                                case "s":
                                    score = uint.Parse(xml.ReadContent());
                                    break;
                                case "c":
                                    count = int.Parse(xml.ReadContent());
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        ref T scoreValue = ref Unsafe.As<uint, T>(ref score);

                        units.Add(new RecordUnit<T>(scoreValue, count));
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            summaries.Add(new SummaryZone<T>(zone, timestamp, units.ToImmutable()));

            _ = xml.SkipEndElement(); // s
        }

        return summaries.ToImmutable();
    }

    private static Summary<T> ReadSummary<T>(ref MiniXmlReader xml) where T : struct
    {
        var timestamp = default(DateTimeOffset);
        var units = ImmutableList.CreateBuilder<RecordUnit<T>>();

        while (xml.TryReadStartElement(out var itemElement))
        {
            switch (itemElement)
            {
                case "d":
                    timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                    break;
                case "i":
                    var score = 0u;
                    var count = 0;

                    while (xml.TryReadStartElement(out var medalElement))
                    {
                        switch (medalElement)
                        {
                            case "s":
                                score = uint.Parse(xml.ReadContent());
                                break;
                            case "c":
                                count = int.Parse(xml.ReadContent());
                                break;
                            default:
                                xml.ReadContent();
                                break;
                        }

                        _ = xml.SkipEndElement();
                    }

                    ref T scoreValue = ref Unsafe.As<uint, T>(ref score);

                    units.Add(new RecordUnit<T>(scoreValue, count));
                    break;
                default:
                    xml.ReadContent();
                    break;
            }

            _ = xml.SkipEndElement();
        }

        return new Summary<T>(timestamp, units.ToImmutable());
    }
}
