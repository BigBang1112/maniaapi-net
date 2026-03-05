using System.Net.Http.Headers;

namespace ManiaAPI.Xml;

public interface IAnyServer : IDisposable
{
    HttpClient Client { get; }

    Task<MasterServerResponse> TestAsync(CancellationToken cancellationToken = default);
}

public abstract class AnyServer : IAnyServer
{
    public HttpClient Client { get; }

    protected abstract string GameXml { get; }

    protected AnyServer(HttpClient client)
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

    protected AnyServer(Uri uri) : this(new HttpClient { BaseAddress = uri }) { }

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

    public virtual async Task<MasterServerResponse> TestAsync(CancellationToken cancellationToken = default)
    {
        const string RequestName = "Test";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, string.Empty, cancellationToken);
        return XmlHelper.ProcessResponseResult(response);
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
