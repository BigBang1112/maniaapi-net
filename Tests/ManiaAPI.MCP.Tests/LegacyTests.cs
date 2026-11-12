using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class LegacyTests
{
    [Test]
    [Arguments("tmuf", "MasterServerTMUF", "game.trackmaniaforever.com")]
    [Arguments("tmnf", "MasterServerTMNF", "game2.trackmaniaforever.com")]
    [Arguments("mp4", "MasterServerMP4", "relay02.v04.maniaplanet.com")]
    public async Task PlayerOperationsUseTheBoundGameServer(string game, string client, string host)
    {
        var fake = FakeHttpMessageHandler.Text("""
            <?xml version="1.0" encoding="UTF-8"?><r><r><n>GetPlayerInfos</n><c><a>player</a><b>Player</b><e>0</e></c></r><e>execution time : 0.0123 s</e></r>
            """, "text/xml");

        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaApiMcp(new ConfigurationBuilder().Build());
        collection.AddHttpClient(client).ConfigurePrimaryHttpMessageHandler(() => fake);

        using var services = collection.BuildServiceProvider();
        using var request = services.CreateScope();
        var options = services.GetRequiredService<McpOptions>();
        var result = await services.GetRequiredService<OperationEngine>().InvokeAsync("masterserver." + game + ".player_infos",
            EngineTests.Json("{\"login\":\"player\"}"), GameScope.Create(options), request.ServiceProvider);

        await Assert.That(result["data"]!["login"]!.GetValue<string>()).IsEqualTo("player");
        await Assert.That(fake.Request.Uri.Host).IsEqualTo(host);
    }

    [Test]
    public async Task DisablingAMasterServerGameHidesItsOperations()
    {
        var options = new McpOptions { MasterServer = new MasterServerOptions { Games = ["MP4"] } };
        var engine = new OperationEngine(options, GeneratedCatalog.Create());
        var operations = engine.Available(GameScope.Create(options)).ToArray();

        await Assert.That(operations.Any(operation => operation.Id.StartsWith("masterserver.tmuf.", StringComparison.Ordinal))).IsFalse();
        await Assert.That(operations.Any(operation => operation.Id.StartsWith("masterserver.mp4.", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task FeaturedToolsCanBeDisabledPerToolset()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaApiMcp(new ConfigurationBuilder().Build());
        collection.Configure<McpOptions>(options => options.Featured["exchange"] = new FeaturedOptions { Tools = [] });

        using var services = collection.BuildServiceProvider();
        var tools = services.GetRequiredService<GatewayTools>().List().Tools;

        await Assert.That(tools.Any(tool => tool.Name is "exchange_search" or "exchange_item")).IsFalse();
        await Assert.That(tools.Any(tool => tool.Name == "maniaapi_invoke")).IsTrue();
    }
}
