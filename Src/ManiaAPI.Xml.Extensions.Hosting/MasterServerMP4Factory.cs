using ManiaAPI.Xml.MP4;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.Xml.Extensions.Hosting;

/// <summary>
/// Factory to create <see cref="MasterServerMP4"/> clients.
/// </summary>
public interface IMasterServerMP4Factory
{
    /// <summary>
    /// Available list of master servers retrieved from the init server. Will be <see langword="null"/> until <see cref="RequestWaitingParamsAsync(CancellationToken)"/> is called, then it has to be manually refreshed.
    /// </summary>
    MasterServerResponse<WaitingParams>? WaitingParams { get; }

    /// <summary>
    /// Requests the list of master servers from the init server. Must be called before creating any clients (<see cref="CreateClient(MasterServerInfo)"/> being an exception). Calling this method again will refresh the <see cref="WaitingParams"/> property, it is not done automatically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result of retrieved master servers.</returns>
    Task<MasterServerResponse<WaitingParams>> RequestWaitingParamsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="MasterServerMP4"/> client for the specified master server. This method does not require <see cref="RequestWaitingParamsAsync(CancellationToken)"/> to be called.
    /// </summary>
    /// <param name="masterServer">The master server info.</param>
    /// <returns>The created client.</returns>
    MasterServerMP4 CreateClient(MasterServerInfo masterServer);

    /// <summary>
    /// Creates a <see cref="MasterServerMP4"/> client for the specified master server name. Requires <see cref="RequestWaitingParamsAsync(CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
    /// <param name="name">The master server name.</param>
    /// <returns>The created client.</returns>
    MasterServerMP4 CreateClient(string name);

    /// <summary>
    /// Creates a <see cref="MasterServerMP4"/> client for the first master server in the list. Requires <see cref="RequestWaitingParamsAsync(CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
    /// <returns>The created client.</returns>
    MasterServerMP4 CreateClient();

    /// <summary>
    /// Creates <see cref="MasterServerMP4"/> clients for all available master servers. Requires <see cref="RequestWaitingParamsAsync(CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
    /// <returns>A dictionary of created clients, keyed by master server name.</returns>
    ImmutableDictionary<string, MasterServerMP4> CreateClients();
}

internal sealed class MasterServerMP4Factory : IMasterServerMP4Factory
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IHttpClientFactory httpClientFactory;

    public MasterServerResponse<WaitingParams>? WaitingParams { get; private set; }

    public MasterServerMP4Factory(IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<MasterServerResponse<WaitingParams>> RequestWaitingParamsAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var initServer = scope.ServiceProvider.GetRequiredService<InitServerMP4>();

        return WaitingParams = await initServer.GetWaitingParamsResponseAsync(cancellationToken);
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

        var masterServerAddress = WaitingParams.Result.MasterServers.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No MasterServer address found with name '{name}'.");

        return CreateClient(masterServerAddress);
    }

    public MasterServerMP4 CreateClient()
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        var masterServerAddress = WaitingParams.Result.MasterServers.FirstOrDefault()
            ?? throw new InvalidOperationException("No MasterServer address found.");

        return CreateClient(masterServerAddress);
    }

    public ImmutableDictionary<string, MasterServerMP4> CreateClients()
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        return WaitingParams.Result.MasterServers.ToImmutableDictionary(x => x.Name, CreateClient);
    }
}
