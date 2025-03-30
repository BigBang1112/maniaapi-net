using MinimalXmlReader;
using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public interface IInitServer : IDisposable
{
    HttpClient Client { get; }

    Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(CancellationToken cancellationToken = default);
    Task<WaitingParams> GetWaitingParamsAsync(CancellationToken cancellationToken = default);
}

public abstract class InitServer : IInitServer
{
    public HttpClient Client { get; }

    protected abstract string GameXml { get; }

    protected InitServer(HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (Xml) by BigBang1112");
    }

    protected InitServer(Uri address) : this(new HttpClient { BaseAddress = address })
    {
    }

    protected async Task<MasterServerResponse<T>> RequestAsync<T>(
        string? authorXml,
        string requestName, 
        string parametersXml, 
        XmlProcessContent<T> processContent, 
        CancellationToken cancellationToken = default) where T : notnull
    {
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml, requestName, parametersXml, cancellationToken);
        return XmlHelper.ProcessResponseResult(requestName, response, processContent);
    }

    public virtual async Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetWaitingParams";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, string.Empty, cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref MiniXmlReader xml) =>
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