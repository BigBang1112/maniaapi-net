using ManiaAPI.MasterServer.TMT;
using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.Tests;

public class AggregatedMasterServerTMTTests
{
    [Test]
    public async Task Dispose_DisposesMasterServerOfEachPlatform()
    {
        var servers = Enum.GetValues<Platform>().ToImmutableDictionary(
            platform => platform,
            platform => new MasterServerTMT(new Uri($"https://{platform}.example.test/")));
        var aggregated = new AggregatedMasterServerTMT(servers);

        aggregated.Dispose();

        foreach (var server in servers.Values)
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(() => server.Client.GetAsync("test"));
        }
    }
}
