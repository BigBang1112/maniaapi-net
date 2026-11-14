using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting;

internal sealed partial class NadeoAPIRefreshBackgroundService : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<NadeoAPIRefreshBackgroundService> logger;
    private readonly TimeProvider timeProvider;

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Failed to refresh the {Service} token. The next request or refresh will try again.")]
    private static partial void LogRefreshFailed(ILogger logger, Exception exception, string service);

    public NadeoAPIRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NadeoAPIRefreshBackgroundService> logger,
        TimeProvider? timeProvider = null)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RefreshInterval, timeProvider, stoppingToken);

            await using var scope = scopeFactory.CreateAsyncScope();

            // Each service is refreshed separately, so one failing doesn't skip the others
            await RefreshAsync<NadeoServices>(scope.ServiceProvider, stoppingToken);
            await RefreshAsync<NadeoLiveServices>(scope.ServiceProvider, stoppingToken);
            await RefreshAsync<NadeoMeetServices>(scope.ServiceProvider, stoppingToken);
        }
    }

    private async Task RefreshAsync<T>(IServiceProvider provider, CancellationToken stoppingToken) where T : NadeoAPI
    {
        try
        {
            if (provider.GetService<T>() is T api)
            {
                await api.RefreshAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // An unhandled exception here would stop the whole host (BackgroundServiceExceptionBehavior.StopHost)
            LogRefreshFailed(logger, ex, typeof(T).Name);
        }
    }
}
