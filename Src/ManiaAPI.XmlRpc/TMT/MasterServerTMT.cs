using MinimalXmlReader;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using TmEssentials;

namespace ManiaAPI.XmlRpc.TMT;

public interface IMasterServerTMT : IMasterServer
{
    Task<MasterServerResponse<ImmutableArray<Summary<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableArray<Summary<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(string zone = "World", CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableArray<Summary<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(params IEnumerable<string> zones);
    Task<ImmutableArray<Summary<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<ImmutableArray<Summary<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", CancellationToken cancellationToken = default);
    Task<ImmutableArray<Summary<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones);
    Task<MasterServerResponse<ImmutableArray<Summary<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableArray<Summary<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableArray<Summary<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, params IEnumerable<string> zones);
    Task<ImmutableArray<Summary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default);
    Task<ImmutableArray<Summary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default);
    Task<ImmutableArray<Summary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones);
}

public class MasterServerTMT : MasterServer, IMasterServerTMT
{
    protected override string GameXml => XmlRpcHelperTMT.GameXml;

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

    public virtual async Task<MasterServerResponse<ImmutableArray<Summary<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetCampaignLeaderBoardSummaries";

        var sb = new StringBuilder("\n<c>TMTurbo@nadeolabs</c>\n<m></m>\n<s>MultiAsyncLevel</s>");
        var i = 1;
        foreach (var zone in zones)
        {
            sb.Append($"\n<z{i}>{zone}</z{i}>");
            i++;
        }

        var response = await XmlRpcHelper.SendAsync(Client, GameXml, RequestName, sb.ToString(), cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, ReadSummaries<int>);
    }

    public async Task<MasterServerResponse<ImmutableArray<Summary<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync([zone], cancellationToken);
    }

    public async Task<MasterServerResponse<ImmutableArray<Summary<int>>>> GetCampaignLeaderBoardSummariesResponseAsync(params IEnumerable<string> zones)
    {
        return await GetCampaignLeaderBoardSummariesResponseAsync(zones, default);
    }

    public async Task<ImmutableArray<Summary<int>>> GetCampaignLeaderBoardSummariesAsync(IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        return (await GetCampaignLeaderBoardSummariesResponseAsync(zones, cancellationToken)).Result;
    }

    public async Task<ImmutableArray<Summary<int>>> GetCampaignLeaderBoardSummariesAsync(string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetCampaignLeaderBoardSummariesAsync([zone], cancellationToken);
    }

    public async Task<ImmutableArray<Summary<int>>> GetCampaignLeaderBoardSummariesAsync(params IEnumerable<string> zones)
    {
        return await GetCampaignLeaderBoardSummariesAsync(zones, default);
    }

    public virtual async Task<MasterServerResponse<ImmutableArray<Summary<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMapLeaderBoardSummaries";

        var sb = new StringBuilder($"\n<c>TMTurbo@nadeolabs</c>\n<m>{mapUid}</m>\n<s>MapTime</s>");
        var i = 1;
        foreach (var zone in zones)
        {
            sb.Append($"\n<z{i}>{zone}</z{i}>");
            i++;
        }

        var response = await XmlRpcHelper.SendAsync(Client, GameXml, RequestName, sb.ToString(), cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, ReadSummaries<TimeInt32>);
    }

    public async Task<MasterServerResponse<ImmutableArray<Summary<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(mapUid, [zone], cancellationToken);
    }

    public async Task<MasterServerResponse<ImmutableArray<Summary<TimeInt32>>>> GetMapLeaderBoardSummariesResponseAsync(string mapUid, params IEnumerable<string> zones)
    {
        return await GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, default);
    }

    public async Task<ImmutableArray<Summary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, IEnumerable<string> zones, CancellationToken cancellationToken = default)
    {
        return (await GetMapLeaderBoardSummariesResponseAsync(mapUid, zones, cancellationToken)).Result;
    }

    public async Task<ImmutableArray<Summary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, string zone = "World", CancellationToken cancellationToken = default)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, [zone], cancellationToken);
    }

    public async Task<ImmutableArray<Summary<TimeInt32>>> GetMapLeaderBoardSummariesAsync(string mapUid, params IEnumerable<string> zones)
    {
        return await GetMapLeaderBoardSummariesAsync(mapUid, zones, default);
    }

    private static ImmutableArray<Summary<T>> ReadSummaries<T>(ref MiniXmlReader xml) where T : struct
    {
        var summaries = ImmutableArray.CreateBuilder<Summary<T>>();

        while (xml.TryReadStartElement("s"))
        {
            var zone = string.Empty;
            var timestamp = DateTimeOffset.UtcNow;
            var medals = ImmutableArray.CreateBuilder<RecordUnit<T>>();

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
                        var score = 0;
                        var count = 0;

                        while (xml.TryReadStartElement(out var medalElement))
                        {
                            switch (medalElement)
                            {
                                case "s":
                                    score = int.Parse(xml.ReadContent());
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

                        ref T scoreValue = ref Unsafe.As<int, T>(ref score);

                        medals.Add(new RecordUnit<T>(scoreValue, count));
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            summaries.Add(new Summary<T>(zone, timestamp, medals.ToImmutable()));

            _ = xml.SkipEndElement(); // s
        }

        return summaries.ToImmutable();
    }
}
