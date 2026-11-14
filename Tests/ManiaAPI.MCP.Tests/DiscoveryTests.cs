using System.Text.Json;
using System.Text.Json.Nodes;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace ManiaAPI.MCP.Tests;

public class DiscoveryTests
{
    [Test]
    [Arguments("TOTD", "nadeo.live.track_of_the_day")]
    [Arguments("COTD", "nadeo.meet.")]
    [Arguments("PB leaderboard", "nadeo.live.")]
    [Arguments("WR map", "nadeo.")]
    public async Task SynonymsFindTheirOperations(string query, string expectedPrefix)
    {
        using var services = CreateServices();
        var result = await Call(services, "maniaapi_search", $$"""{"query":"{{query}}","limit":3}""");
        var first = result["operations"]![0]!.GetValue<string>();

        await Assert.That(first).StartsWith(expectedPrefix);
    }

    [Test]
    public async Task SignaturesUseCompactTypes()
    {
        using var services = CreateServices();
        var result = await Call(services, "maniaapi_search", """{"query":"player zone","limit":5}""");
        var line = result["operations"]!.AsArray().Select(x => x!.GetValue<string>()).First(x => x.StartsWith("nadeo.core.player_zone(", StringComparison.Ordinal));

        await Assert.That(line).Contains("accountId: guid");
        await Assert.That(line).DoesNotContain("System.");
    }

    [Test]
    public async Task TurboSignaturesShowThePlatformArgument()
    {
        using var services = CreateServices();
        var result = await Call(services, "maniaapi_search", """{"query":"player infos","toolset":"trackmaniaturbo","limit":5}""");
        var line = result["operations"]!.AsArray().Select(x => x!.GetValue<string>())
            .First(x => x.StartsWith("masterserver.tmt.player_infos(", StringComparison.Ordinal));

        await Assert.That(line).StartsWith("masterserver.tmt.player_infos(platform: PC|XB1|PS4; login: string)");
    }

    [Test]
    public async Task MissingRequiredArgumentsAreNamed()
    {
        using var services = CreateServices();
        var result = await services.GetRequiredService<GatewayTools>().CallAsync(new CallToolRequestParams
        {
            Name = "maniaapi_invoke",
            Arguments = JsonDocument.Parse("""{"operation":"masterserver.tmt.player_infos","arguments":{"login":"bigbang1112"}}""")
                .RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.Clone())
        }, services, CancellationToken.None);

        var message = result.StructuredContent!.Value.GetProperty("message").GetString();

        await Assert.That(result.IsError).IsEqualTo(true);
        await Assert.That(message).Contains("platform");
        await Assert.That(message).DoesNotContain("validation failed");
    }

    [Test]
    public async Task ParametersCarryDescriptionsAndDefaults()
    {
        var operation = GeneratedCatalog.Create().Single(x => x.Id == "nadeo.live.top_leaderboard");
        var properties = operation.Schema([], false)["properties"]!;

        // From the XML doc's <param> when written, otherwise from the parameter-name override; defaults from C#.
        await Assert.That(properties["groupUid"]!["description"]!.GetValue<string>()).Contains("Personal_Best");
        await Assert.That(properties["mapUid"]!["description"]!.GetValue<string>()).IsEqualTo("The UID of the map.");
        await Assert.That(CatalogMetadata.Parameter<int>("clubId", null)["description"]!.GetValue<string>()).IsEqualTo("Club ID.");
        await Assert.That(properties["length"]!["default"]!.GetValue<int>()).IsEqualTo(10);
        await Assert.That(properties["onlyWorld"]!["default"]!.GetValue<bool>()).IsTrue();
    }

    [Test]
    public async Task DescribeGivesGuidanceAndRelatedOperationsInScope()
    {
        using var services = CreateServices();
        var result = await Call(services, "maniaapi_describe", """{"operations":["nadeo.live.top_leaderboard","masterserver.tmnf.player_infos","tmx.track_gbx_response"]}""");
        var leaderboard = result["operations"]![0]!;
        var player = result["operations"]![1]!;
        var file = result["operations"]![2]!;
        var related = leaderboard["relatedOperations"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();

        await Assert.That(leaderboard["whenToUse"]!.GetValue<string>()).IsNotEqualTo(leaderboard["summary"]!.GetValue<string>());
        await Assert.That(related[0]).IsEqualTo("nadeo.live.surrounding_records");
        await Assert.That(related).Contains("trackmania.api.display_names");

        // TMNF shares the TMUF overrides, rewritten to its own operations.
        await Assert.That(player["relatedOperations"]![0]!.GetValue<string>()).IsEqualTo("masterserver.tmnf.check_login");
        await Assert.That(player["whenToUse"]!.GetValue<string>()).Contains("login");

        // A file download points to the operation that summarizes the same file.
        await Assert.That(file["relatedOperations"]![0]!.GetValue<string>()).IsEqualTo("tmx.track_gbx");
        await Assert.That(file["whenToUse"]!.GetValue<string>()).StartsWith("Downloads the file");
    }

    [Test]
    public async Task DescribeReportsOnlyActualPaginationParameters()
    {
        using var services = CreateServices();
        var result = await Call(services, "maniaapi_describe", """{"operations":["nadeo.live.top_leaderboard","nadeo.core.player_zone","mx.search_maps"]}""");
        string[] Pagination(int index) => result["operations"]![index]!["pagination"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();

        await Assert.That(Pagination(0)).IsEquivalentTo(["length", "offset"]);

        // accountId contains "count", but isn't a page size.
        await Assert.That(Pagination(1)).IsEmpty();

        // Exchange searches page through fields of their parameters object.
        await Assert.That(Pagination(2)).IsEquivalentTo(["count", "after", "before"]);
    }

    [Test]
    public async Task MasterServerOperationsHaveNoSeparateTimedDuplicates()
    {
        var catalog = GeneratedCatalog.Create();

        await Assert.That(catalog.Any(x => x.Id.StartsWith("masterserver.", StringComparison.Ordinal) &&
            x.Id.EndsWith("_response", StringComparison.Ordinal))).IsFalse();

        // Convenience overloads without a timed variant stay callable.
        var lookup = catalog.Single(x => x.Id == "masterserver.mp4.web_identity_from_maniaplanet_login");
        await Assert.That(lookup.Overloads.Select(x => x.Signature)).IsEquivalentTo(["logins: string[]", "login: string"]);
    }

    [Test]
    public async Task RelatedOperationsStayInsideTheEndpointScope()
    {
        // Without TrackmaniaAPI credentials, display_names isn't available and must not be suggested.
        using var services = CreateServices(trackmaniaApi: false);
        var result = await Call(services, "maniaapi_describe", """{"operations":["nadeo.live.top_leaderboard"]}""");
        var related = result["operations"]![0]!["relatedOperations"]!.AsArray().Select(x => x!.GetValue<string>());

        await Assert.That(related).DoesNotContain("trackmania.api.display_names");
    }

    [Test]
    public async Task OverridesReferenceExistingOperations()
    {
        var ids = GeneratedCatalog.Create().Concat(GbxOperations.Create()).Concat(ScoreUrlOperations.Create())
            .Select(x => x.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var id in ids)
        {
            foreach (var related in OperationOverrides.Related(id))
            {
                await Assert.That(ids).Contains(related).Because($"{id} lists a related operation that doesn't exist");
            }
        }
    }

    private static ServiceProvider CreateServices(bool trackmaniaApi = true)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ManiaAPI:NadeoAPI:Login"] = "test-login",
            ["ManiaAPI:NadeoAPI:Password"] = "test-password"
        };

        if (trackmaniaApi)
        {
            settings["ManiaAPI:TrackmaniaAPI:ClientId"] = "test-client";
            settings["ManiaAPI:TrackmaniaAPI:ClientSecret"] = "test-secret";
        }

        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaAPI(TestConfiguration.Create(settings));

        return collection.BuildServiceProvider();
    }

    private static async Task<JsonNode> Call(IServiceProvider services, string tool, string arguments)
    {
        var parsed = JsonDocument.Parse(arguments).RootElement.EnumerateObject()
            .ToDictionary(x => x.Name, x => x.Value.Clone());

        var result = await services.GetRequiredService<GatewayTools>().CallAsync(
            new CallToolRequestParams { Name = tool, Arguments = parsed }, services, CancellationToken.None);

        if (result.IsError == true)
        {
            throw new InvalidOperationException(result.StructuredContent!.Value.GetRawText());
        }

        return JsonNode.Parse(result.StructuredContent!.Value.GetRawText())!;
    }
}
