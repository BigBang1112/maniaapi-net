using MinimalXmlReader;
using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc;

public interface IMasterServer : IDisposable
{
    HttpClient Client { get; }

    Task<MasterServerResponse<ImmutableArray<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default);
    Task<ImmutableArray<League>> GetLeaguesAsync(CancellationToken cancellationToken = default);
}

public abstract class MasterServer : IMasterServer
{
    public HttpClient Client { get; }

    protected abstract string GameXml { get; }

    protected MasterServer(HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (XmlRpc) by BigBang1112");
    }

    protected MasterServer(Uri address) : this(new HttpClient { BaseAddress = address })
    {
    }

    protected async Task<MasterServerResponse<T>> RequestAsync<T>(
        string? authorXml,
        string requestName,
        string parametersXml,
        XmlRpcProcessContent<T> processContent,
        CancellationToken cancellationToken = default) where T : notnull
    {
        var response = await XmlRpcHelper.SendAsync(Client, GameXml, authorXml, requestName, parametersXml, cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(requestName, response, processContent);
    }

    public virtual async Task<MasterServerResponse<ImmutableArray<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetLeagues";
        var response = await XmlRpcHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, string.Empty, cancellationToken);
        return XmlRpcHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var items = ImmutableArray.CreateBuilder<League>();

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

                    _ = xml.SkipEndElement();
                }

                items.Add(new League(name, path, logoUrl));

                _ = xml.SkipEndElement(); // l
            }

            return items.ToImmutable();
        });
    }

    public async Task<ImmutableArray<League>> GetLeaguesAsync(CancellationToken cancellationToken = default)
    {
        return (await GetLeaguesResponseAsync(cancellationToken)).Result;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
