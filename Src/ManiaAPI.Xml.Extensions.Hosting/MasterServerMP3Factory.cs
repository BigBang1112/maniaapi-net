using ManiaAPI.Xml.MP3;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.Xml.Extensions.Hosting;

/// <summary>
/// Factory to create <see cref="MasterServerMP3"/> clients.
/// </summary>
public interface IMasterServerMP3Factory : IMasterServerMPFactory<MasterServerMP3>;

internal sealed class MasterServerMP3Factory : MasterServerMPFactory<InitServerMP3, MasterServerMP3>, IMasterServerMP3Factory
{
    private readonly IHttpClientFactory httpClientFactory;

    public MasterServerMP3Factory(IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory)
        : base(serviceScopeFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public override MasterServerMP3 CreateClient(MasterServerInfo masterServer)
    {
        var client = httpClientFactory.CreateClient(nameof(MasterServerMP3));
        client.BaseAddress = masterServer.GetUri();
        return new MasterServerMP3(client);
    }
}
