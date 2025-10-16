using ManiaAPI.Xml.MP4;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.Xml.Extensions.Hosting;

public interface IMasterServerMP4Factory
{
    WaitingParams? WaitingParams { get; }

    Task RequestWaitingParamsAsync(CancellationToken cancellationToken = default);
    MasterServerMP4 CreateClient(MasterServerInfo masterServer);
    MasterServerMP4 CreateClient(string name);
    MasterServerMP4 CreateClient();
    ImmutableDictionary<string, MasterServerMP4> CreateClients();
}

internal sealed class MasterServerMP4Factory : IMasterServerMP4Factory
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IHttpClientFactory httpClientFactory;

    public WaitingParams? WaitingParams { get; private set; }

    public MasterServerMP4Factory(IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task RequestWaitingParamsAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var initServer = scope.ServiceProvider.GetRequiredService<InitServerMP4>();

        WaitingParams = await initServer.GetWaitingParamsAsync(cancellationToken);
    }

    public MasterServerMP4 CreateClient(MasterServerInfo masterServer)
    {
        return new MasterServerMP4(masterServer.GetUri(), httpClientFactory.CreateClient(nameof(MasterServerMP4)));
    }

    public MasterServerMP4 CreateClient(string name)
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        var masterServerAddress = WaitingParams.MasterServers.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No MasterServer address found with name '{name}'.");

        return CreateClient(masterServerAddress);
    }

    public MasterServerMP4 CreateClient()
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        var masterServerAddress = WaitingParams.MasterServers.FirstOrDefault()
            ?? throw new InvalidOperationException("No MasterServer address found.");

        return CreateClient(masterServerAddress);
    }

    public ImmutableDictionary<string, MasterServerMP4> CreateClients()
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        return WaitingParams.MasterServers.ToImmutableDictionary(x => x.Name, CreateClient);
    }
}
