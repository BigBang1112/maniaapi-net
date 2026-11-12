using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ManiaAPI.MCP.Hosting;

namespace ManiaAPI.MCP.Tests;

public class CatalogTests
{
    [Test]
    public async Task CatalogMatchesApprovedSurface()
    {
        var actual = GeneratedCatalog.Create().Concat(GbxOperations.Create()).Concat(ScoreUrlOperations.Create())
            .OrderBy(x => x.Id, StringComparer.Ordinal)
            .Select(operation => operation.Id + " [" + operation.Requires + ", serviceOnly=" + operation.ServiceAccountOnly +
                ", readOnly=" + operation.ReadOnly + ", playerOnly=" + operation.PlayerOnly + "] " +
                string.Join(" || ", operation.Overloads.Select(overload => overload.Signature)))
            .Select(line => line.TrimEnd())
            .ToArray();

        var expected = await File.ReadAllLinesAsync(Path.Combine(AppContext.BaseDirectory, "Catalog.approved.txt"));

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ToolListStaysInsideContextBudget()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaApiMcp(new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ManiaApiMcp:Nadeo:Accounts:ServiceAccount:Login"] = "test-login",
                ["ManiaApiMcp:Nadeo:Accounts:ServiceAccount:Password"] = "test-password",
                ["ManiaApiMcp:TrackmaniaApi:ClientId"] = "test-client",
                ["ManiaApiMcp:TrackmaniaApi:ClientSecret"] = "test-secret"
            }).Build());

        using var services = collection.BuildServiceProvider();
        var list = services.GetRequiredService<GatewayTools>().List();
        var characters = list.Tools.Sum(tool => tool.Name.Length + (tool.Description?.Length ?? 0) +
            tool.InputSchema.GetRawText().Length + (tool.OutputSchema?.GetRawText().Length ?? 0) + 200);
        var estimatedTokens = (characters + 2) / 3;

        await Assert.That(list.Tools.Count).IsLessThanOrEqualTo(20);
        await Assert.That(estimatedTokens).IsLessThan(8000);
    }

    [Test]
    public async Task LiveTestPermissionsOverrideInterfaceComments()
    {
        var catalog = GeneratedCatalog.Create();

        await Assert.That(catalog.Single(x => x.Id == "nadeo.core.player_club_tags").ServiceAccountOnly).IsTrue();
        await Assert.That(catalog.Single(x => x.Id == "nadeo.core.account_records_by_map_ids").ServiceAccountOnly).IsFalse();
    }

    [Test]
    public async Task Tm2SearchKeepsTitleContextAndOnlyMappedSite()
    {
        using var services = EngineTests.CreateServices();
        var gateway = services.GetRequiredService<GatewayTools>();

        var result = await gateway.CallAsync(new ModelContextProtocol.Protocol.CallToolRequestParams
        {
            Name = "maniaapi_search",
            Arguments = new Dictionary<string, System.Text.Json.JsonElement>
            {
                ["query"] = EngineTests.Json("\"community maps title TMStadium environment Stadium\""),
                ["game"] = EngineTests.Json("\"TM2\"")
            }
        }, services, CancellationToken.None);

        var json = result.StructuredContent!.Value.GetRawText();

        await Assert.That(result.IsError ?? false).IsFalse();
        await Assert.That(json).Contains("title TMStadium environment Stadium");
        await Assert.That(json).Contains("site: Maniaplanet");
        await Assert.That(json).DoesNotContain("site: Shootmania");
        await Assert.That(json).DoesNotContain("site: Trackmania");
    }

    [Test]
    public async Task DisabledSiteIsAbsentFromEveryDiscoverySchema()
    {
        var options = new McpOptions
        {
            Toolsets = ["exchange"],
            Exchange = new ExchangeOptions { Sites = [], MxSites = ["Trackmania"] }
        };

        var scope = GameScope.Create(options);
        var engine = new OperationEngine(options, GeneratedCatalog.Create());
        var visible = engine.Available(scope).ToArray();

        await Assert.That(visible.All(x => x.ExchangeClient == "mx")).IsTrue();

        foreach (var operation in visible)
        {
            var schema = operation.Schema(scope.Sites(operation), false).ToJsonString();

            await Assert.That(schema).DoesNotContain("Shootmania");
            await Assert.That(schema).DoesNotContain("Maniaplanet");
        }
    }
}
