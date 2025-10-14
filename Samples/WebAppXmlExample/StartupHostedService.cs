using ManiaAPI.Xml.Extensions.Hosting;

namespace WebAppXmlExample;

public sealed class StartupHostedService : IHostedService
{
    private readonly IMasterServerTMTFactory masterServerTMTFactory;

    public StartupHostedService(IMasterServerTMTFactory masterServerTMTFactory)
    {
        this.masterServerTMTFactory = masterServerTMTFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await masterServerTMTFactory.RequestWaitingParamsAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
