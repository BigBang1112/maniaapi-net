using ManiaAPI.XmlRpc;
using ManiaAPI.XmlRpc.TMT;

var initServer = new InitServerTMT(Enum.Parse<Platform>(args[0]));

var waitingParams = await initServer.GetWaitingParamsAsync();

var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

internal class ExtendedMasterServerTMT : MasterServerTMT
{
    public ExtendedMasterServerTMT(MasterServerInfo info) : base(info)
    {
    }

    public async Task GetChallengeRecordsComparisonAsync(string login, CancellationToken cancellationToken = default)
    {

    }

    /*public async Task<ServerInfoTMT> GetServerInfoAsync(string serverLogin)
    {
        const string RequestName = "GetWaitingParams";
        var response = await XmlRpcHelper.SendAsync(Client, GameXml, RequestName, string.Empty, cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var masterServers = ImmutableArray.CreateBuilder<MasterServerInfo>();

            while (xml.TryReadStartElement(out var element))
            {
                switch (element)
                {
                    case "ms":
                        var name = string.Empty;
                        var domain = string.Empty;
                        var path = string.Empty;

                        while (xml.TryReadStartElement(out var valueElement))
                        {
                            switch (valueElement)
                            {
                                case "b":
                                    name = xml.ReadContentAsString();
                                    break;
                                case "c":
                                    domain = xml.ReadContentAsString();
                                    break;
                                case "d":
                                    path = xml.ReadContentAsString();
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        masterServers.Add(new MasterServerInfo(name, domain, path));
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            return new WaitingParams(masterServers.ToImmutable());
        });
    }*/
}