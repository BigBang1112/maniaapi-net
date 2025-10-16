using ManiaAPI.Xml.Extensions.Hosting;

namespace WebAppXmlExample;

public sealed class StartupHostedService : IHostedService
{
    private readonly IMasterServerTMTFactory masterServerTMTFactory;
    private readonly IMasterServerMP4Factory masterServerMP4Factory;

    public StartupHostedService(IMasterServerTMTFactory masterServerTMTFactory, IMasterServerMP4Factory masterServerMP4Factory)
    {
        this.masterServerTMTFactory = masterServerTMTFactory;
        this.masterServerMP4Factory = masterServerMP4Factory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await masterServerTMTFactory.RequestWaitingParamsAsync(cancellationToken);
        await masterServerMP4Factory.RequestWaitingParamsAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
