using MinimalXmlReader;

namespace ManiaAPI.XmlRpc;

public interface IInitServer : IDisposable
{
    HttpClient Client { get; }
}

public abstract class InitServer : IInitServer
{
    public HttpClient Client { get; }

    protected abstract string GameXml { get; }

    protected InitServer(HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (XmlRpc) by BigBang1112");
    }

    protected InitServer(Uri address) : this(new HttpClient { BaseAddress = address })
    {
    }

    public virtual async Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetWaitingParams";
        var responseStr = await XmlRpcHelper.SendAsync(Client, GameXml, RequestName, string.Empty, cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, responseStr, (ref MiniXmlReader xml) =>
        {
            var masterServers = new List<MasterServerInfo>();

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

            return new WaitingParams(masterServers);
        });
    }

    public async Task<WaitingParams> GetWaitingParamsAsync(CancellationToken cancellationToken = default)
    {
        return (await GetWaitingParamsResponseAsync(cancellationToken)).Result;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}