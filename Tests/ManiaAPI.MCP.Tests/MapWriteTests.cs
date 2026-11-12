using System.Text.Json.Nodes;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.NadeoAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class MapWriteTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ConfirmedMapWritesAdaptLocalFilesIntoMultipartRequests(bool update)
    {
        var fake = FakeHttpMessageHandler.Json("""
            {"mapId":"00000000-0000-0000-0000-000000000001","mapUid":"test","name":"Test",
             "timestamp":"2026-01-01T00:00:00Z","fileUrl":"https://example.test/map.Map.Gbx","thumbnailUrl":"https://example.test/image.jpg"}
            """);

        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaApiMcp(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ManiaApiMcp:AllowWrites"] = "true",
            ["ManiaApiMcp:Nadeo:Accounts:ServiceAccount:Login"] = "service-login",
            ["ManiaApiMcp:Nadeo:Accounts:ServiceAccount:Password"] = "service-password"
        }).Build());

        collection.AddSingleton(_ => new NadeoServices(fake.CreateClient(), new NadeoAPIHandler(), automaticallyAuthorize: false));
        collection.AddSingleton<INadeoAccounts>(provider => new FakeAccounts(provider));

        using var services = collection.BuildServiceProvider();
        using var request = services.CreateScope();
        var input = new JsonObject
        {
            ["source"] = new JsonObject { ["path"] = Path.Combine(AppContext.BaseDirectory, "Test.Map.Gbx") }
        };

        if (update)
        {
            input["mapId"] = "00000000-0000-0000-0000-000000000001";
        }

        var result = await services.GetRequiredService<OperationEngine>().InvokeAsync(
            update ? "nadeo.core.update_map" : "nadeo.core.upload_map", EngineTests.Json(input.ToJsonString()),
            GameScope.Create(services.GetRequiredService<McpOptions>()), request.ServiceProvider, confirmed: true);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Headers["Content-Type"]).Contains("multipart/form-data");
        await Assert.That(fake.Request.Uri.AbsolutePath).IsEqualTo(update ? "/maps/00000000-0000-0000-0000-000000000001" : "/maps/");
        await Assert.That(result["data"]!["mapUid"]!.GetValue<string>()).IsEqualTo("test");
    }

    private sealed class FakeAccounts(IServiceProvider provider) : INadeoAccounts
    {
        public IServiceProvider Get(string? account) => provider;
    }
}
