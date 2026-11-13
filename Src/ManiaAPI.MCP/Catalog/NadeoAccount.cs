using ManiaAPI.MCP.Configuration;
using ManiaAPI.NadeoAPI;
using ManiaAPI.NadeoAPI.Extensions.Hosting;

namespace ManiaAPI.MCP.Catalog;

// The NadeoAPI clients live in their own container, so the hosting package's audience-keyed token
// handlers never mix with the rest of the server. Clients still authorize and refresh lazily.
public sealed class NadeoAccount : INadeoAccount, IHostedService, IDisposable
{
    private readonly ServiceProvider? provider;

    public NadeoAccount(McpOptions options)
    {
        if (!options.NadeoAPI.Configured)
        {
            return;
        }

        var services = new ServiceCollection();
        services.AddLogging();

        services.AddNadeoAPI(o =>
        {
            o.Credentials = new NadeoAPICredentials(options.NadeoAPI.Login!, options.NadeoAPI.Password!);
            o.UserAgent = options.NadeoAPI.UserAgent;
        });

        provider = services.BuildServiceProvider();
    }

    public IServiceProvider Services => provider ?? throw new ArgumentException("The NadeoAPI account is not configured.");

    public IEnumerable<DateTimeOffset> Expirations()
    {
        if (provider is null)
        {
            yield break;
        }

        foreach (var audience in new[] { nameof(NadeoServices), nameof(NadeoLiveServices), nameof(NadeoMeetServices) })
        {
            if (provider.GetRequiredKeyedService<NadeoAPIHandler>(audience).JWT?.ExpirationTime is { } expiration)
            {
                yield return expiration;
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var service in provider?.GetServices<IHostedService>() ?? [])
        {
            await service.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var service in provider?.GetServices<IHostedService>() ?? [])
        {
            await service.StopAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        provider?.Dispose();
    }
}
