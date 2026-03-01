using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.Xml.Extensions.Hosting;

/// <summary>
/// Factory to create <see cref="MasterServerMP"/> clients.
/// </summary>
public interface IMasterServerMPFactory<T> where T : IMasterServerMP
{
    /// <summary>
    /// Available list of master servers retrieved from the init server. Will be <see langword="null"/> until <see cref="RequestWaitingParamsAsync(string?, CancellationToken)"/> is called, then it has to be manually refreshed.
    /// </summary>
    MasterServerResponse<WaitingParams>? WaitingParams { get; }

    /// <summary>
    /// Requests the list of master servers from the init server. Must be called before creating any clients (<see cref="CreateClient(MasterServerInfo)"/> being an exception). Calling this method again will refresh the <see cref="WaitingParams"/> property, it is not done automatically.
    /// </summary>
    /// <param name="login">Optional player login to determine the master server.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result of retrieved master servers.</returns>
    Task<MasterServerResponse<WaitingParams>> RequestWaitingParamsAsync(string? login = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <typeparamref name="T"/> client for the specified master server. This method does not require <see cref="RequestWaitingParamsAsync(string?, CancellationToken)"/> to be called.
    /// </summary>
    /// <param name="masterServer">The master server info.</param>
    /// <returns>The created client.</returns>
    T CreateClient(MasterServerInfo masterServer);

    /// <summary>
    /// Creates a <typeparamref name="T"/> client for the specified master server name. Requires <see cref="RequestWaitingParamsAsync(string?, CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
    /// <param name="name">The master server name.</param>
    /// <returns>The created client.</returns>
    T CreateClient(string name);

    /// <summary>
    /// Creates a <typeparamref name="T"/> client for the first master server in the list. Requires <see cref="RequestWaitingParamsAsync(string?, CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
    /// <returns>The created client.</returns>
    T CreateClient();

    /// <summary>
    /// Creates <typeparamref name="T"/> clients for all available master servers. Requires <see cref="RequestWaitingParamsAsync(string?, CancellationToken)"/> to be called first (at least once at the start of the application).
    /// </summary>
    /// <returns>A dictionary of created clients, keyed by master server name.</returns>
    ImmutableDictionary<string, T> CreateClients();
}

internal abstract class MasterServerMPFactory<TInit, TMaster> : IMasterServerMPFactory<TMaster> where TInit : InitServerMP where TMaster : MasterServerMP
{
    private readonly IServiceScopeFactory serviceScopeFactory;

    public MasterServerResponse<WaitingParams>? WaitingParams { get; private set; }

    public MasterServerMPFactory(IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<MasterServerResponse<WaitingParams>> RequestWaitingParamsAsync(string? login = null, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var initServer = scope.ServiceProvider.GetRequiredService<TInit>();

        return WaitingParams = await initServer.GetWaitingParamsResponseAsync(login, cancellationToken);
    }

    public abstract TMaster CreateClient(MasterServerInfo masterServer);

    public TMaster CreateClient(string name)
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        var masterServer = WaitingParams.Result.MasterServers.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"No MasterServer address found with name '{name}'.");

        return CreateClient(masterServer);
    }

    public TMaster CreateClient()
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        var masterServerAddress = WaitingParams.Result.MasterServers.FirstOrDefault()
            ?? throw new InvalidOperationException("No MasterServer address found.");

        return CreateClient(masterServerAddress);
    }

    public ImmutableDictionary<string, TMaster> CreateClients()
    {
        if (WaitingParams is null)
        {
            throw new InvalidOperationException($"WaitingParams not set. Call {nameof(RequestWaitingParamsAsync)} first.");
        }

        return WaitingParams.Result.MasterServers.ToImmutableDictionary(x => x.Name, CreateClient);
    }
}
