using ManiaAPI.Xml.TMT;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.Xml.Extensions.Hosting;

/// <summary>
/// Factory to create <see cref="MasterServerTMT"/> clients.
/// </summary>
public interface IMasterServerTMTFactory
{
    /// <summary>
    /// Available list of master servers for each platform retrieved from the init server. Will be empty until <see cref="RequestWaitingParamsAsync(CancellationToken)"/> is called, then it has to be manually refreshed.
    /// </summary>
    ImmutableDictionary<Platform, MasterServerResponse<WaitingParams>> WaitingParams { get; }

    /// <summary>
    /// Requests the list of master servers from the init servers. Must be called before creating any clients. Calling this method again will refresh the <see cref="WaitingParams"/> property, it is not done automatically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    Task RequestWaitingParamsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="MasterServerTMT"/> client for the specified platform. Requires <see cref="RequestWaitingParamsAsync(CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
    /// <param name="platform">The master server info.</param>
    /// <returns>The created client.</returns>
    MasterServerTMT CreateClient(Platform platform);

    /// <summary>
    /// Creates <see cref="MasterServerTMT"/> clients for all available platforms. Requires <see cref="RequestWaitingParamsAsync(CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
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
