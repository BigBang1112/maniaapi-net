using ManiaAPI.XmlRpc.TMT;

namespace WebAppXmlRpcExample;

public sealed class StartupHostedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;

    public StartupHostedService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        foreach (var platform in Enum.GetValues<Platform>())
        {
            var initServer = scope.ServiceProvider.GetRequiredKeyedService<InitServerTMT>(platform);
            var waitingParams = await initServer.GetWaitingParamsAsync(cancellationToken);
            var masterServer = scope.ServiceProvider.GetRequiredKeyedService<MasterServerTMT>(platform);
            masterServer.Client.BaseAddress = waitingParams.MasterServers.First().GetUri();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
