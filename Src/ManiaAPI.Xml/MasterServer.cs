using MinimalXmlReader;
using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Reflection.Metadata;

namespace ManiaAPI.Xml;

public interface IMasterServer : IDisposable
{
    HttpClient Client { get; }

    Task<MasterServerResponse<ImmutableList<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default);
    Task<ImmutableList<League>> GetLeaguesAsync(CancellationToken cancellationToken = default);
    Task<MasterServerResponse<PlayerInfos>> GetPlayerInfosResponseAsync(string login, CancellationToken cancellationToken = default);
    Task<PlayerInfos> GetPlayerInfosAsync(string login, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<CheckLoginResult>> CheckLoginResponseAsync(string login, CancellationToken cancellationToken = default);
    Task<CheckLoginResult> CheckLoginAsync(string login, CancellationToken cancellationToken = default);
}

public abstract class MasterServer : IMasterServer
{
    public HttpClient Client { get; }

    protected Uri ServerUri { get; }
    protected abstract string GameXml { get; }

    protected MasterServer(Uri uri, HttpClient client)
    {
        Client = client;

        var headers = Client.DefaultRequestHeaders;

        const string product = "ManiaAPI.NET";
        const string version = "2.7.0";

        var libraryExists = headers.UserAgent.Any(h => h.Product?.Name == product && h.Product?.Version == version);

        if (!libraryExists)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(product, version));
            headers.UserAgent.Add(new ProductInfoHeaderValue("(Xml; Email=petrpiv1@gmail.com; Discord=bigbang1112)"));
        }

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

    public async Task<MasterServerResponse<PlayerInfos>> GetPlayerInfosResponseAsync(string login, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetPlayerInfos";
        var parametersXml = $"<login>{login}</login>";
        var response = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml: null, RequestName, parametersXml, cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var nickname = string.Empty;
            var zone = string.Empty;
            var createdAt = default(DateTimeOffset?);
            var d = 0;
            var lastZoneUpdate = default(DateTimeOffset?);
            var k = default(int?);

            while (xml.TryReadStartElement(out var infoElement))
            {
                switch (infoElement)
                {
                    case "a":
                        login = xml.ReadContentAsString();
                        break;
                    case "b":
                        nickname = xml.ReadContentAsString();
                        break;
                    case "c":
                        zone = xml.ReadContentAsString();
                        break;
                    case "d":
                        d = int.Parse(xml.ReadContent());
                        break;
                    case "e":
                        var e = long.Parse(xml.ReadContent());
                        createdAt = e == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(e);
                        break;
                    case "j":
                        lastZoneUpdate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                        break;
                    case "k":
                        k = int.Parse(xml.ReadContent());
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }
                _ = xml.SkipEndElement();
            }

            return new PlayerInfos(login, nickname, zone, createdAt, d, lastZoneUpdate, k);
        });
    }

    public async Task<PlayerInfos> GetPlayerInfosAsync(string login, CancellationToken cancellationToken = default)
    {
        return (await GetPlayerInfosResponseAsync(login, cancellationToken)).Result;
    }

    public async Task<MasterServerResponse<CheckLoginResult>> CheckLoginResponseAsync(string login, CancellationToken cancellationToken = default)
    {
        const string RequestName = "CheckLogin";
        var parametersXml = $"<l>{login}</l>";
        var response = await XmlHelper.SendAsync(Client, ServerUri, GameXml, authorXml: null, RequestName, parametersXml, cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
        {
            var exists = false;
            var paid = default(bool?);
            var migrated = false;

            while (xml.TryReadStartElement(out var infoElement))
            {
                switch (infoElement)
                {
                    case "e":
                        exists = MemoryExtensions.Equals(xml.ReadContent(), "1", StringComparison.Ordinal);
                        break;
                    case "p":
                        paid = xml.ReadContentAsBoolean();
                        break;
                    case "m":
                        migrated = MemoryExtensions.Equals(xml.ReadContent(), "1", StringComparison.Ordinal);
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }
                _ = xml.SkipEndElement();
            }

            return new CheckLoginResult(exists, paid, migrated);
        });
    }

    public async Task<CheckLoginResult> CheckLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return (await CheckLoginResponseAsync(login, cancellationToken)).Result;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
