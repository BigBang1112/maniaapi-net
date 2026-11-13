using System.Collections.Immutable;
using ManiaAPI.MasterServer.Extensions.Hosting;
using ManiaAPI.MasterServer.TMT;

namespace ManiaAPI.MCP.Hosting;

public sealed class LegacyClients(IServiceProvider services) : IDisposable
{
    private readonly Dictionary<Platform, MasterServerTMT> turbo = [];

    public async Task<object> TurboAsync(string? platform, CancellationToken cancellationToken)
    {
        if (turbo.Count == 0)
        {
            var factory = services.GetRequiredService<IMasterServerTMTFactory>();
            await factory.RequestWaitingParamsAsync(cancellationToken: cancellationToken);

            foreach (var (key, client) in factory.CreateClients())
            {
                turbo.Add(key, client);
            }
        }

        if (platform is null)
        {
            return new AggregatedMasterServerTMT(turbo.ToImmutableDictionary());
        }

        if (!Enum.TryParse<Platform>(platform, out var selected) || !Enum.IsDefined(selected))
        {
            throw new ArgumentException("platform must be one of: " + string.Join(", ", Enum.GetNames<Platform>()) + ".");
        }

        if (!turbo.TryGetValue(selected, out var result))
        {
            throw new ArgumentException($"The {selected} master server isn't available right now; available: " +
                (turbo.Count == 0 ? "none" : string.Join(", ", turbo.Keys.Order())) + ".");
        }

        return result;
    }

    public void Dispose()
    {
        foreach (var client in turbo.Values)
        {
            client.Dispose();
        }
    }
}
