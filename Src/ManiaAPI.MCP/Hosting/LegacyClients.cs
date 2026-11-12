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

        if (!Enum.TryParse<Platform>(platform, out var selected) || !turbo.TryGetValue(selected, out var result))
        {
            throw new ArgumentException("platform must be PC, XB1, or PS4.");
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
