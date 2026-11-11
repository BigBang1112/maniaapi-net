using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting.Tests;

/// <summary>
/// A host with <see cref="NadeoAPIServiceExtensions.AddNadeoAPI(IServiceCollection, Action{NadeoAPIOptions}, Action{IHttpClientBuilder}?, Action{IHttpClientBuilder}?, Action{IHttpClientBuilder}?)"/>,
/// whose clients talk to <see cref="FakeNadeoAuthServer"/> and whose background refresh waits on <see cref="FakeTimeProvider"/>.
/// </summary>
internal sealed class RefreshHost : IAsyncDisposable
{
    // Matches the interval of the background refresh
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    private readonly IHost host;

    public FakeNadeoAuthServer Server { get; } = new();
    public FakeTimeProvider Time { get; } = new();
    public LogCollector Logs { get; } = new();

    public bool IsStopping => host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.IsCancellationRequested;

    public Task RefreshTask => host.Services.GetServices<IHostedService>().OfType<BackgroundService>().Single().ExecuteTask!;

    public IEnumerable<LogEntry> RefreshWarnings => Logs.Entries
        .Where(x => x.Level == LogLevel.Warning && x.Category.EndsWith("NadeoAPIRefreshBackgroundService", StringComparison.Ordinal));

    public RefreshHost()
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);
        builder.Logging.AddProvider(Logs);
        builder.Services.AddSingleton<TimeProvider>(Time);
        builder.Services.AddNadeoAPI(_ => { }, UseFakeServer, UseFakeServer, UseFakeServer);
        host = builder.Build();
    }

    private void UseFakeServer(IHttpClientBuilder builder)
    {
        builder.ConfigurePrimaryHttpMessageHandler(() => Server);
    }

    public async Task<T> AuthorizeAsync<T>() where T : NadeoAPI
    {
        var api = host.Services.GetRequiredService<T>();
        await api.AuthorizeAsync("login", "password");
        return api;
    }

    public T Get<T>() where T : notnull => host.Services.GetRequiredService<T>();

    public Task StartAsync() => host.StartAsync();

    public Task StopAsync() => host.StopAsync();

    /// <summary>
    /// Moves the clock one refresh interval at a time until the condition is met.
    /// </summary>
    /// <remarks>
    /// Advancing repeatedly instead of once covers the moments when the service isn't waiting yet,
    /// like right after start or while a refresh is still running.
    /// </remarks>
    public async Task AdvanceUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + WaitTimeout;

        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException($"The condition wasn't met within {WaitTimeout.TotalSeconds} seconds.");
            }

            Time.Advance(RefreshInterval);
            await Task.Delay(10);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await host.StopAsync();
        host.Dispose();
    }
}
