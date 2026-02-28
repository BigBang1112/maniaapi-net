using System.Collections.Immutable;
using System.Net.Http.Headers;

namespace ManiaAPI.Xml;

public interface IInitServerMP : IDisposable
{
    HttpClient Client { get; }

    Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string? login = null, CancellationToken cancellationToken = default);
    Task<WaitingParams> GetWaitingParamsAsync(string? login = null, CancellationToken cancellationToken = default);
    Task<MasterServerResponse> TestAsync(CancellationToken cancellationToken = default);
}

public abstract class InitServerMP : IInitServerMP
{
    public HttpClient Client { get; }

    protected abstract string GameXml { get; }

    protected InitServerMP(HttpClient client)
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

        if (client.BaseAddress is null)
        {
            throw new ArgumentException("BaseAddress must be set for ManiaPlanet InitServer", nameof(client));
        }
    }

    protected InitServerMP(Uri uri) : this(new HttpClient { BaseAddress = uri }) { }

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

    public virtual async Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string? login = null, CancellationToken cancellationToken = default)
    {
        return await GetWaitingParamsResponseAsync(Client, GameXml, login, cancellationToken);
    }

    public async Task<WaitingParams> GetWaitingParamsAsync(string? login = null, CancellationToken cancellationToken = default)
    {
        return (await GetWaitingParamsResponseAsync(login, cancellationToken)).Result;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual async Task<MasterServerResponse> TestAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "Test";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, string.Empty, cancellationToken);
        return XmlHelper.ProcessResponseResult(response);
    }

    internal static async Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(HttpClient client, string gameXml, string? login = null, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetWaitingParams";
        var authorXml = login is null ? null : $"<author><login>{login}</login></author>";
        var response = await XmlHelper.SendAsync(client, gameXml, authorXml, RequestName, string.Empty, cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
        {
            var waitingQueueDuration = 0;
            var waitingQueueMessage = default(string);
            var masterServers = ImmutableList.CreateBuilder<MasterServerInfo>();

            while (xml.TryReadStartElement(out var element))
            {
                switch (element)
                {
                    case "wt":
                        waitingQueueDuration = int.Parse(xml.ReadContent());
                        break;
                    case "wm":
                        waitingQueueMessage = xml.ReadContentAsString();
                        break;
                    case "ms":
                        var name = string.Empty;
                        var domain = string.Empty;
                        var path = string.Empty;
                        var portHttps = 443;
                        var portHttp = 80;

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
                                case "e":
                                    portHttps = int.Parse(xml.ReadContent());
                                    break;
                                case "f":
                                    portHttp = int.Parse(xml.ReadContent());
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        masterServers.Add(new MasterServerInfo(name, domain, path, portHttps, portHttp));
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            return new WaitingParams(waitingQueueDuration, waitingQueueMessage, masterServers.ToImmutable());
        });
    }
}