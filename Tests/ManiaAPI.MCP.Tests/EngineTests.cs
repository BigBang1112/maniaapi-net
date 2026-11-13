using System.Text.Json;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class EngineTests
{
    internal static ServiceProvider CreateServices(FakeHttpMessageHandler? handler = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddManiaAPI(TestConfiguration.Create());

        if (handler is not null)
        {
            services.AddHttpClient("MX_Trackmania").ConfigurePrimaryHttpMessageHandler(() => handler);
        }

        return services.BuildServiceProvider();
    }

    internal static JsonElement Json(string value)
    {
        using var document = JsonDocument.Parse(value);

        return document.RootElement.Clone();
    }

    [Test]
    public async Task MissingCredentialsHideOnlyAuthenticatedOperations()
    {
        using var services = CreateServices();
        var engine = services.GetRequiredService<OperationEngine>();
        var operations = engine.Available(GameScope.Create(new McpOptions())).ToArray();

        await Assert.That(operations.Length).IsGreaterThan(20);
        await Assert.That(operations.All(x => x.Requires == "none")).IsTrue();
        await Assert.That(operations.All(x => x.Toolset != "trackmania2020")).IsTrue();
    }

    [Test]
    public async Task CatalogClassifiesWritesAndPlayerRoutesWithoutExposingAuthorization()
    {
        var catalog = GeneratedCatalog.Create();
        var ids = catalog.Select(x => x.Id).ToArray();

        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Length);
        await Assert.That(catalog.Single(x => x.Id == "trackmania.api.user_favorite_maps").PlayerOnly).IsTrue();
        await Assert.That(catalog.Single(x => x.Id == "nadeo.meet.send_matchmaking_heartbeat").ReadOnly).IsFalse();
        await Assert.That(ids.Any(x => x.Contains("authorize") || x.Contains("authenticate"))).IsFalse();
        await Assert.That(catalog.Count(x => x.Requires == "oauth" && !x.PlayerOnly)).IsEqualTo(2);
    }

    [Test]
    public async Task GameEndpointInfersItsSingleSiteAndCallsTypedClient()
    {
        var fake = FakeHttpMessageHandler.Json("[\"Race\",\"Stunt\"]");
        using var services = CreateServices(fake);
        using var request = services.CreateScope();
        var engine = services.GetRequiredService<OperationEngine>();

        var result = await engine.InvokeAsync("mx.maptypes", Json("{}"),
            GameScope.Create(new McpOptions(), "trackmania2020"), request.ServiceProvider);

        await Assert.That(fake.Request.Uri.Host).IsEqualTo("trackmania.exchange");
        await Assert.That(result["data"]!.AsArray().Count).IsEqualTo(2);
    }

    [Test]
    public async Task GeneralEndpointRequiresExplicitSite()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var services = CreateServices(fake);
        var engine = services.GetRequiredService<OperationEngine>();

        await Assert.That(async () => await engine.InvokeAsync("mx.maptypes", Json("{}"),
            GameScope.Create(new McpOptions()), services)).Throws<ArgumentException>();

        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task CrossGameSiteNeverCallsUpstream()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var services = CreateServices(fake);
        var engine = services.GetRequiredService<OperationEngine>();

        await Assert.That(async () => await engine.InvokeAsync("mx.maptypes", Json("""
            {"site":"Shootmania"}
            """), GameScope.Create(new McpOptions(), "trackmania2020"), services)).Throws<ArgumentException>();

        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task SearchParameterObjectBindsWithoutReflection()
    {
        var fake = FakeHttpMessageHandler.Json("{\"Results\":[],\"More\":false}");
        using var services = CreateServices(fake);
        using var request = services.CreateScope();
        var engine = services.GetRequiredService<OperationEngine>();

        await engine.InvokeAsync("mx.search_maps", Json("""
            {"site":"Trackmania","parameters":{"count":3,"name":"test"}}
            """), GameScope.Create(new McpOptions()), request.ServiceProvider);

        await Assert.That(fake.Request.Url).Contains("count=3");
    }

    [Test]
    public async Task InvalidFieldIsRejectedBeforeUpstream()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var services = CreateServices(fake);
        var engine = services.GetRequiredService<OperationEngine>();

        await Assert.That(async () => await engine.InvokeAsync("mx.search_maps", Json("""
            {"site":"Trackmania","parameters":{"count":"three"}}
            """), GameScope.Create(new McpOptions()), services)).Throws<ArgumentException>();

        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task ZoneLeaderboardRejectsPaging()
    {
        var options = new McpOptions();
        options.NadeoAPI.Login = "test";
        options.NadeoAPI.Password = "test";

        var engine = new OperationEngine(options, GeneratedCatalog.Create());
        using var services = CreateServices();

        await Assert.That(async () => await engine.InvokeAsync("nadeo.live.top_leaderboard", Json("""
            {"mapUid":"map","groupUid":"Personal_Best","zoneId":"00000000-0000-0000-0000-000000000001","length":5}
            """), GameScope.Create(options), services)).Throws<ArgumentException>()
            .WithMessageContaining("Valid combinations: (");
    }

    [Test]
    public async Task UpstreamErrorsDoNotLeakSecretMessages()
    {
        var error = OperationEngine.Error(new HttpRequestException("password=secret", null, HttpStatusCode.ServiceUnavailable));

        await Assert.That(error.ToJsonString()).DoesNotContain("secret");
        await Assert.That(error["retryable"]!.GetValue<bool>()).IsTrue();
    }
}
