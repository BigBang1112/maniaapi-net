using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting;

internal class NadeoAPIRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;

    public NadeoAPIRefreshBackgroundService(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(20), stoppingToken);

            await using var scope = scopeFactory.CreateAsyncScope();

            if (scope.ServiceProvider.GetService<NadeoServices>() is NadeoServices ns)
            {
                await ns.RefreshAsync(stoppingToken);
            }

            if (scope.ServiceProvider.GetService<NadeoLiveServices>() is NadeoLiveServices nls)
            {
                await nls.RefreshAsync(stoppingToken);
            }

            if (scope.ServiceProvider.GetService<NadeoMeetServices>() is NadeoMeetServices nms)
            {
                await nms.RefreshAsync(stoppingToken);
            }
        }
    }
}
