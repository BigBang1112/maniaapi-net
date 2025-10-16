using ManiaAPI.Xml.TMT;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.Xml.Extensions.Hosting;

public interface IMasterServerTMTFactory
{
    ImmutableDictionary<Platform, MasterServerResponse<WaitingParams>> WaitingParams { get; }

    Task RequestWaitingParamsAsync(CancellationToken cancellationToken = default);
    MasterServerTMT CreateClient(Platform platform);
    ImmutableDictionary<Platform, MasterServerTMT> CreateClients();
}

internal sealed class MasterServerTMTFactory : IMasterServerTMTFactory
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IHttpClientFactory httpClientFactory;

    public ImmutableDictionary<Platform, MasterServerResponse<WaitingParams>> WaitingParams { get; private set; } = ImmutableDictionary<Platform, MasterServerResponse<WaitingParams>>.Empty;

    public MasterServerTMTFactory(IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task RequestWaitingParamsAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var initServers = scope.ServiceProvider.GetRequiredService<ImmutableDictionary<Platform, InitServerTMT>>();

        var tasks = initServers.ToDictionary(
            x => x.Value.GetWaitingParamsResponseAsync(cancellationToken),
            x => x.Key);

        await foreach (var (platform, task) in tasks.WhenEachRemove())
        {
            WaitingParams = WaitingParams.Add(platform, await task);
        }
    }

    public MasterServerTMT CreateClient(Platform platform)
    {
        if (!WaitingParams.TryGetValue(platform, out var paramsForPlatform))
        {
            throw new InvalidOperationException($"WaitingParams for platform {platform} not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        var masterServerAddress = paramsForPlatform.Result.MasterServers.FirstOrDefault()
            ?? throw new InvalidOperationException($"No MasterServer address found for platform {platform}.");

        return new MasterServerTMT(masterServerAddress.GetUri(), httpClientFactory.CreateClient($"{nameof(MasterServerTMT)}_{platform}"));
    }

    public ImmutableDictionary<Platform, MasterServerTMT> CreateClients()
    {
        return Enum.GetValues<Platform>().ToImmutableDictionary(x => x, CreateClient);
    }
}
