using MinimalXmlReader;
using System.Diagnostics;

namespace ManiaAPI.XmlRpc;

public interface IMasterServer : IDisposable
{
    HttpClient Client { get; }

    Task<IReadOnlyCollection<League>> GetLeaguesAsync(CancellationToken cancellationToken = default);
    Task<MasterServerResponse<IReadOnlyCollection<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default);
}

public abstract class MasterServer : IMasterServer
{
    public HttpClient Client { get; }

    protected MasterServer(HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (XmlRpc) by BigBang1112");
    }

    protected MasterServer(Uri address) : this(new HttpClient { BaseAddress = address })
    {
    }

    protected abstract Task<string> SendAsync(string requestName, string parameters, CancellationToken cancellationToken);

    public virtual async Task<MasterServerResponse<IReadOnlyCollection<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetLeagues";
        var responseStr = await SendAsync(RequestName, string.Empty, cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, responseStr, (ref MiniXmlReader xml) =>
        {
            var items = new List<League>();

            while (xml.TryReadStartElement("l"))
            {
                var name = string.Empty;
                var path = string.Empty;
                var logoUrl = string.Empty;

                while (xml.TryReadStartElement(out var itemElement))
                {
                    switch (itemElement)
                    {
                        case "a":
                            name = xml.ReadContentAsString();
                            break;
                        case "b":
                            path = xml.ReadContentAsString();
                            break;
                        case "i":
                            logoUrl = xml.ReadContentAsString();
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    Debug.Assert(xml.SkipEndElement());
                }

                items.Add(new League(name, path, logoUrl));

                Debug.Assert(xml.SkipEndElement()); // l
            }

            return (IReadOnlyCollection<League>)items;
        });
    }

    public async Task<IReadOnlyCollection<League>> GetLeaguesAsync(CancellationToken cancellationToken = default)
    {
        return (await GetLeaguesResponseAsync(cancellationToken)).Result;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
