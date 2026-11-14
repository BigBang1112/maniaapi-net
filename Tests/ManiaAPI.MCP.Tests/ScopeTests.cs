using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;

namespace ManiaAPI.MCP.Tests;

public class ScopeTests
{
    [Test]
    [Arguments("TM2", "Trackmania 2", "Maniaplanet")]
    [Arguments("TrackMania²", "Trackmania 2", "Maniaplanet")]
    [Arguments("trackmania 2", "Trackmania 2", "Maniaplanet")]
    [Arguments("tM-2", "Trackmania 2", "Maniaplanet")]
    [Arguments("ＴＭ２", "Trackmania 2", "Maniaplanet")]
    [Arguments("ShootMania", "ShootMania", "Shootmania")]
    [Arguments("s m", "ShootMania", "Shootmania")]
    public async Task AliasesRetainFamily(string alias, string family, string site)
    {
        var context = GameAliases.Resolve(alias);

        await Assert.That(context.Toolset).IsEqualTo("maniaplanet");
        await Assert.That(context.Family).IsEqualTo(family);
        await Assert.That(context.MxSites).IsEquivalentTo([site]);
    }

    [Test]
    public async Task GameEndpointNarrowsSites()
    {
        var scope = GameScope.Create(new McpOptions(), "trackmania2020");

        await Assert.That(scope.Toolsets).Contains("exchange");
        await Assert.That(scope.MxSites).IsEquivalentTo(["Trackmania"]);
        await Assert.That(scope.TmxSites).IsEmpty();
    }

    [Test]
    public async Task RemovingExchangeRemovesMappedSites()
    {
        var options = new McpOptions { Toolsets = ["trackmania2020"] };
        var scope = GameScope.Create(options, "trackmania2020");

        await Assert.That(scope.Toolsets).DoesNotContain("exchange");
        await Assert.That(scope.MxSites).IsEmpty();
    }

    [Test]
    public async Task FilterDoesNotChangeInvocationScope()
    {
        var scope = GameScope.Create(new McpOptions());
        var filtered = scope.ForGame("TM2");

        await Assert.That(filtered.MxSites).IsEquivalentTo(["Maniaplanet"]);
        await Assert.That(scope.MxSites.Length).IsEqualTo(3);
        await Assert.That(scope.InferSite).IsFalse();
    }

    [Test]
    public async Task OutOfScopeAliasGivesEndpointGuidance()
    {
        var scope = GameScope.Create(new McpOptions(), "trackmania2020");

        await Assert.That(() => scope.ForGame("TM2"))
            .Throws<ArgumentException>()
            .WithMessageContaining("/mcp/maniaplanet");
    }

    [Test]
    [Arguments("Stadium")]
    [Arguments("unknown game")]
    public async Task AmbiguousOrUnknownGameFails(string game)
    {
        await Assert.That(() => GameAliases.Resolve(game)).Throws<ArgumentException>();
    }
}
