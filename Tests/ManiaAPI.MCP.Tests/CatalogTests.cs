using System.Text.Json.Nodes;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using ManiaAPI.MCP.Hosting;

namespace ManiaAPI.MCP.Tests;

public class CatalogTests
{
    /// <remarks>
    /// After an intended catalog change, run the tests with MANIAAPI_APPROVE_CATALOG=1 to rewrite
    /// Catalog.approved.txt, then review its diff.
    /// </remarks>
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

        if (Environment.GetEnvironmentVariable("MANIAAPI_APPROVE_CATALOG") == "1")
        {
            await File.WriteAllLinesAsync(Path.Combine(SourceDirectory(), "Catalog.approved.txt"), actual);
        }

        var expected = await File.ReadAllLinesAsync(Path.Combine(AppContext.BaseDirectory, "Catalog.approved.txt"));

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ToolListStaysInsideContextBudget()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaAPI(TestConfiguration.Create(
            new Dictionary<string, string?>
            {
                ["ManiaAPI:NadeoAPI:Login"] = "test-login",
                ["ManiaAPI:NadeoAPI:Password"] = "test-password",
                ["ManiaAPI:TrackmaniaAPI:ClientId"] = "test-client",
                ["ManiaAPI:TrackmaniaAPI:ClientSecret"] = "test-secret"
            }));

        using var services = collection.BuildServiceProvider();
        var list = services.GetRequiredService<GatewayTools>().List();
        var characters = list.Tools.Sum(tool => tool.Name.Length + (tool.Description?.Length ?? 0) +
            tool.InputSchema.GetRawText().Length + (tool.OutputSchema?.GetRawText().Length ?? 0) + 200);
        var estimatedTokens = (characters + 2) / 3;

        await Assert.That(list.Tools.Count).IsLessThanOrEqualTo(20);
        await Assert.That(estimatedTokens).IsLessThan(8000);
    }

    [Test]
    public async Task ToolSchemasAreFlatObjectsWithResolvableReferences()
    {
        // Clients such as the Anthropic API reject anyOf/oneOf/allOf at the top level of an input schema.
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaAPI(TestConfiguration.Create(
            new Dictionary<string, string?>
            {
                ["ManiaAPI:ExposeOperationsAsTools"] = "true",
                ["ManiaAPI:NadeoAPI:Login"] = "test-login",
                ["ManiaAPI:NadeoAPI:Password"] = "test-password",
                ["ManiaAPI:TrackmaniaAPI:ClientId"] = "test-client",
                ["ManiaAPI:TrackmaniaAPI:ClientSecret"] = "test-secret"
            }));

        using var services = collection.BuildServiceProvider();
        var tools = services.GetRequiredService<GatewayTools>().List().Tools;

        await Assert.That(tools.Count).IsGreaterThan(100);

        foreach (var tool in tools)
        {
            var schema = JsonNode.Parse(tool.InputSchema.GetRawText())!.AsObject();

            await Assert.That(schema["type"]!.GetValue<string>()).IsEqualTo("object");
            await Assert.That(schema.Any(x => x.Key is "anyOf" or "oneOf" or "allOf")).IsFalse()
                .Because($"{tool.Name} has a top-level combinator");

            foreach (var reference in References(schema))
            {
                await Assert.That(Resolve(schema, reference)).IsNotNull()
                    .Because($"{tool.Name} has an unresolved reference {reference}");
            }
        }

        var leaderboard = JsonNode.Parse(tools.Single(x => x.Name == "trackmania2020_leaderboard").InputSchema.GetRawText())!;
        var modes = leaderboard["properties"]!["mode"]!["enum"]!.AsArray().Select(x => x!.GetValue<string>());

        await Assert.That(modes).IsEquivalentTo(["top", "around", "position"]);
        await Assert.That(leaderboard["properties"]!["zoneId"]).IsNotNull();
        await Assert.That(leaderboard["required"]!.AsArray().Select(x => x!.GetValue<string>())).DoesNotContain("mode");
    }

    private static string SourceDirectory([System.Runtime.CompilerServices.CallerFilePath] string path = "")
    {
        return Path.GetDirectoryName(path)!;
    }

    private static IEnumerable<string> References(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj["$ref"] is JsonValue value && value.TryGetValue<string>(out var reference))
            {
                yield return reference;
            }

            foreach (var child in obj.SelectMany(x => References(x.Value)))
            {
                yield return child;
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var child in array.SelectMany(References))
            {
                yield return child;
            }
        }
    }

    private static JsonNode? Resolve(JsonNode root, string reference)
    {
        if (!reference.StartsWith('#'))
        {
            return null;
        }

        var node = (JsonNode?)root;

        foreach (var segment in reference[1..].Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            var name = segment.Replace("~1", "/").Replace("~0", "~");

            node = node switch
            {
                JsonObject obj => obj[name],
                JsonArray array when int.TryParse(name, out var index) && index < array.Count => array[index],
                _ => null
            };
        }

        return node;
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
