using ManiaAPI.Xml.MP4;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.Xml.Extensions.Hosting;

/// <summary>
/// Factory to create <see cref="MasterServerMP4"/> clients.
/// </summary>
public interface IMasterServerMP4Factory : IMasterServerMPFactory<MasterServerMP4>;

internal sealed class MasterServerMP4Factory : MasterServerMPFactory<InitServerMP4, MasterServerMP4>, IMasterServerMP4Factory
{
    private readonly IHttpClientFactory httpClientFactory;

    public MasterServerMP4Factory(IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory)
        : base(serviceScopeFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public override MasterServerMP4 CreateClient(MasterServerInfo masterServer)
    {
        var client = httpClientFactory.CreateClient(nameof(MasterServerMP4));
        client.BaseAddress = masterServer.GetUri();
        return new MasterServerMP4(client);
    }
}
