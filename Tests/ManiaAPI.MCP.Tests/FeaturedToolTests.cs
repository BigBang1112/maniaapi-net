using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using ManiaAPI.NadeoAPI;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class FeaturedToolTests
{
    private const string AccountId = "00000000-0000-0000-0000-000000000001";

    [Test]
    public async Task PlayerReturnsZoneWhenTheAccountIsDeniedTrophiesAndClubTags()
    {
        // A dedicated server login is denied the trophy summary and club tags with 401.
        var fake = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath.Contains("/trophies/", StringComparison.Ordinal) ||
            request.RequestUri.AbsolutePath.Contains("/clubTags/", StringComparison.Ordinal)
                ? FakeResponse.Json("""{"error":"denied"}""", HttpStatusCode.Unauthorized)
                : FakeResponse.Json($$"""[{"accountId":"{{AccountId}}","timestamp":"2026-01-01T00:00:00Z","zoneId":"{{AccountId}}"}]"""));

        using var services = CreateServices(fake);
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<FeaturedTools>().InvokeAsync("trackmania2020_player",
            EngineTests.Json($$"""{"accountId":"{{AccountId}}"}"""),
            GameScope.Create(services.GetRequiredService<McpOptions>()), request.ServiceProvider, CancellationToken.None);

        var notes = result["truncation"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();

        await Assert.That(result["data"]!["zone"]!["zoneId"]!.GetValue<string>()).IsEqualTo(AccountId);
        await Assert.That(result["data"]!["trophies"]).IsNull();
        await Assert.That(result["data"]!["clubTags"]).IsNull();
        await Assert.That(notes.Any(x => x.StartsWith("trophies unavailable:", StringComparison.Ordinal) &&
            x.Contains("service account", StringComparison.Ordinal))).IsTrue();
        await Assert.That(notes.Any(x => x.StartsWith("clubTags unavailable:", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    [Arguments("{}")]
    [Arguments($$"""{"accountId":"{{AccountId}}","displayName":"name"}""")]
    public async Task PlayerRequiresExactlyOneIdentifier(string arguments)
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var services = CreateServices(fake);

        await Assert.That(async () => await services.GetRequiredService<FeaturedTools>().InvokeAsync("trackmania2020_player",
                EngineTests.Json(arguments), GameScope.Create(services.GetRequiredService<McpOptions>()), services, CancellationToken.None))
            .Throws<ArgumentException>().WithMessageContaining("exactly one");

        await Assert.That(fake.Requests).IsEmpty();
    }

    private static ServiceProvider CreateServices(FakeHttpMessageHandler fake)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaAPI(TestConfiguration.Create(new Dictionary<string, string?>
        {
            ["ManiaAPI:NadeoAPI:Login"] = "dedicated-login",
            ["ManiaAPI:NadeoAPI:Password"] = "dedicated-password"
        }));

        collection.AddSingleton(_ => new NadeoServices(fake.CreateClient(), new NadeoAPIHandler(), automaticallyAuthorize: false));
        collection.AddSingleton<INadeoAccount>(provider => new FakeAccount(provider));

        return collection.BuildServiceProvider();
    }

    private sealed class FakeAccount(IServiceProvider provider) : INadeoAccount
    {
        public IServiceProvider Services => provider;
    }
}
