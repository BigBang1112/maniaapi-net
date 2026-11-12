using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ManiaAPI.MCP.Tools;
using ModelContextProtocol.Protocol;

namespace ManiaAPI.MCP.Tests;

[LiveTest]
public class LiveGatewayTests
{
    [Test]
    public async Task ExchangeRead()
    {
        using var services = EngineTests.CreateServices();
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<GatewayTools>().CallAsync(new CallToolRequestParams
        {
            Name = "exchange_search",
            Arguments = new Dictionary<string, System.Text.Json.JsonElement>
            {
                ["site"] = EngineTests.Json("\"Trackmania\""),
                ["parameters"] = EngineTests.Json("{\"count\":1}")
            }
        }, request.ServiceProvider, CancellationToken.None);

        await Assert.That(result.IsError ?? false).IsFalse();
    }

    [Test]
    public async Task Trackmania2020Read()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ManiaApiMcp:Nadeo:Accounts:ServiceAccount:Login"] = LiveSettings.NadeoAPI.ServiceAccount.Login,
            ["ManiaApiMcp:Nadeo:Accounts:ServiceAccount:Password"] = LiveSettings.NadeoAPI.ServiceAccount.Password
        }).Build();

        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaApiMcp(configuration);

        using var services = collection.BuildServiceProvider();
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<GatewayTools>().CallAsync(new CallToolRequestParams
        {
            Name = "trackmania2020_campaign",
            Arguments = new Dictionary<string, System.Text.Json.JsonElement> { ["length"] = EngineTests.Json("1") }
        }, request.ServiceProvider, CancellationToken.None);

        await Assert.That(result.IsError ?? false).IsFalse();
    }

    [Test]
    [Arguments("tmuf_player")]
    [Arguments("tmnf_player")]
    [Arguments("maniaplanet_player")]
    [Arguments("trackmaniaturbo_leaderboard")]
    public async Task LegacyFeaturedRead(string tool)
    {
        using var services = EngineTests.CreateServices();
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<GatewayTools>().CallAsync(new CallToolRequestParams
        {
            Name = tool,
            Arguments = tool == "trackmaniaturbo_leaderboard"
                ? new Dictionary<string, System.Text.Json.JsonElement>()
                : new Dictionary<string, System.Text.Json.JsonElement> { ["login"] = EngineTests.Json("\"bigbang1112\"") }
        }, request.ServiceProvider, CancellationToken.None);

        await Assert.That(result.IsError ?? false).IsFalse();
    }
}
