using MinimalXmlReader;
using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public interface IMasterServer : IDisposable
{
    HttpClient Client { get; }

    Task<MasterServerResponse<ImmutableList<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default);
    Task<ImmutableList<League>> GetLeaguesAsync(CancellationToken cancellationToken = default);
}

public abstract class MasterServer : IMasterServer
{
    public HttpClient Client { get; }

    protected Uri ServerUri { get; }
    protected abstract string GameXml { get; }

    protected MasterServer(Uri uri, HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NET/2.5.0 (Xml; Email=petrpiv1@gmail.com; Discord=bigbang1112)");

        ServerUri = uri;
    }

    protected MasterServer(Uri uri) : this(uri, new HttpClient())
    {
    }

    protected async Task<MasterServerResponse<T>> RequestAsync<T>(
        string? authorXml,
        string requestName,
        string parametersXml,
        XmlProcessContent<T> processContent,
        CancellationToken cancellationToken = default) where T : notnull
    {
        var response = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml, requestName, parametersXml, cancellationToken);
        return XmlHelper.ProcessResponseResult(requestName, response, processContent);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetLeagues";
        var response = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml: null, RequestName, string.Empty, cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var items = ImmutableList.CreateBuilder<League>();

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

    public async Task<ImmutableList<League>> GetLeaguesAsync(CancellationToken cancellationToken = default)
    {
        return (await GetLeaguesResponseAsync(cancellationToken)).Result;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
