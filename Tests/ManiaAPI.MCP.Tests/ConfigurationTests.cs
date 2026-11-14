using System.Security.Cryptography;
using System.Text;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;
using ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ManiaAPI.MCP.Tests;

public class ConfigurationTests
{
    [Test]
    public async Task MissingUpstreamCredentialsAreValid()
    {
        var result = new McpOptionsValidator().Validate(null, new McpOptions { UserAgent = TestConfiguration.UserAgent });

        await Assert.That(result.Succeeded).IsTrue();
    }

    [Test]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task MissingUserAgentFailsValidation(string? userAgent)
    {
        var result = new McpOptionsValidator().Validate(null, new McpOptions { UserAgent = userAgent });

        await Assert.That(result.Failed).IsTrue();
        await Assert.That(result.FailureMessage!).Contains("Set ManiaAPI:UserAgent");
    }

    [Test]
    public async Task ServerWithoutUserAgentDoesNotStart()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddManiaAPI(new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();

        await Assert.That(() => provider.GetRequiredService<McpOptions>())
            .Throws<Microsoft.Extensions.Options.OptionsValidationException>()
            .WithMessageContaining("ManiaAPI:UserAgent");
    }

    [Test]
    public async Task InvalidNamesDoNotEchoSecrets()
    {
        var options = new McpOptions
        {
            Toolsets = ["secret-value"],
            Exchange = new ExchangeOptions { Sites = ["secret-site"] }
        };

        var result = new McpOptionsValidator().Validate(null, options);

        await Assert.That(result.Failed).IsTrue();
        await Assert.That(result.FailureMessage!).DoesNotContain("secret-value");
        await Assert.That(result.FailureMessage!).DoesNotContain("secret-site");
    }

    [Test]
    public async Task HttpRequiresKey()
    {
        await Assert.That(() => HttpSecurity.Validate(new McpOptions(), ["http://localhost:5199"]))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task NonLoopbackWritesAreRejected()
    {
        var options = new McpOptions
        {
            AllowWrites = true,
            Http = new HttpOptions { ApiKeys = ["test-key"] }
        };

        await Assert.That(() => HttpSecurity.Validate(options, ["http://0.0.0.0:5199"]))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task KeyComparisonAcceptsOnlyConfiguredKeys()
    {
        byte[][] hashes = [SHA256.HashData(Encoding.UTF8.GetBytes("test-key"))];

        await Assert.That(HttpSecurity.Authenticate("invalid", hashes, out _)).IsFalse();
        await Assert.That(HttpSecurity.Authenticate("test-key", hashes, out var identity)).IsTrue();
        await Assert.That(identity).IsEqualTo(0);
    }

    [Test]
    public async Task AllToolsetsAreEnabledByDefault()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddManiaAPI(TestConfiguration.Create());

        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<McpOptions>().Toolsets).IsEquivalentTo(McpOptions.SupportedToolsets);
    }

    [Test]
    public async Task ConfiguredArraysReplaceDefaults()
    {
        var configuration = TestConfiguration.Create(new Dictionary<string, string?>
        {
            ["ManiaAPI:Toolsets:0"] = "exchange",
            ["ManiaAPI:Exchange:Sites:0"] = "TMNF"
        });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddManiaAPI(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<McpOptions>();

        await Assert.That(options.Toolsets).IsEquivalentTo(["exchange"]);
        await Assert.That(options.Exchange.Sites).IsEquivalentTo(["TMNF"]);
        await Assert.That(options.Exchange.MxSites).IsEquivalentTo(["Trackmania", "Maniaplanet", "Shootmania"]);
    }

    [Test]
    public async Task ConfiguredAccountLinkingScopesReplaceDefaults()
    {
        var directory = Path.Combine(Path.GetTempPath(), "maniaapi-mcp-" + Guid.NewGuid().ToString("N"));
        var configuration = TestConfiguration.Create(new Dictionary<string, string?>
        {
            ["ManiaAPI:TrackmaniaAPI:ClientId"] = "client",
            ["ManiaAPI:TrackmaniaAPI:ClientSecret"] = "secret",
            ["ManiaAPI:ManiaPlanet:ClientId"] = "client",
            ["ManiaAPI:ManiaPlanet:ClientSecret"] = "secret",
            ["ManiaAPI:AccountLinking:Enabled"] = "true",
            ["ManiaAPI:AccountLinking:PublicOrigin"] = "https://localhost",
            ["ManiaAPI:AccountLinking:StorePath"] = Path.Combine(directory, "tokens"),
            ["ManiaAPI:AccountLinking:DataProtectionKeyPath"] = Path.Combine(directory, "keys"),
            ["ManiaAPI:AccountLinking:TrackmaniaScopes:0"] = "clubs",
            ["ManiaAPI:AccountLinking:ManiaPlanetScopes:0"] = "basic"
        });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddManiaAPI(configuration);

        using var provider = services.BuildServiceProvider();

        var trackmania = provider.GetRequiredService<IOptionsMonitor<TrackmaniaAuthenticationOptions>>().Get("LinkTrackmania");
        var maniaPlanet = provider.GetRequiredService<IOptionsMonitor<ManiaPlanetAuthenticationOptions>>().Get("LinkManiaplanet");

        await Assert.That(trackmania.Scope).IsEquivalentTo(["clubs"]);
        await Assert.That(maniaPlanet.Scope).IsEquivalentTo(["basic"]);
    }

    [Test]
    public async Task CommandLineToolsetsReplaceConfiguredArray()
    {
        var configuration = TestConfiguration.Create(new Dictionary<string, string?>
        {
            ["ManiaAPI:Toolsets:0"] = "trackmania2020",
            ["ManiaAPI:Toolsets:1"] = "exchange"
        });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddManiaAPI(configuration, ["exchange"]);

        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<McpOptions>().Toolsets).IsEquivalentTo(["exchange"]);
    }

    [Test]
    public async Task ReadinessNeverAuthorizesCredentialsOnProbe()
    {
        var options = new McpOptions
        {
            TrackmaniaAPI = new OAuthOptions { ClientId = "test", ClientSecret = "test" }
        };

        var health = new UpstreamHealth(options);

        await Assert.That(health.Ready).IsTrue();
        await Assert.That(health.Snapshot()["oauth:trackmania"]!.GetValue<string>()).IsEqualTo("notYetAuthorized");

        health.Record("oauth", authorized: false);
        await Assert.That(health.Ready).IsFalse();

        health.Record("oauth", authorized: true);
        await Assert.That(health.Ready).IsTrue();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task OnlyCredentialsThatCannotAuthorizeFailReadiness(bool credentialsWork)
    {
        // With working credentials, the 401 comes from the operation itself (a permission denial for this
        // account) and must not take the instance out of rotation. Without them, authorization fails.
        var payload = $$"""
            {"jti":"j","aud":"a","iat":0,"nbf":0,"exp":{{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}},"sub":"s","scopes":[]}
            """;
        var token = "header." + Base64Url(payload) + ".signature";

        var fake = new FakeHttpMessageHandler(request => request.RequestUri!.AbsolutePath.EndsWith("/access_token", StringComparison.Ordinal)
            ? credentialsWork
                ? FakeResponse.Json($$"""{"token_type":"Bearer","expires_in":3600,"access_token":"{{token}}"}""")
                : FakeResponse.Json("""{"error":"invalid_client"}""", HttpStatusCode.Unauthorized)
            : FakeResponse.Json("""{"error":"denied"}""", HttpStatusCode.Unauthorized));

        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaAPI(TestConfiguration.Create(new Dictionary<string, string?>
        {
            ["ManiaAPI:TrackmaniaAPI:ClientId"] = "test-client",
            ["ManiaAPI:TrackmaniaAPI:ClientSecret"] = "test-secret"
        }));
        collection.AddHttpClient("TrackmaniaAPI").ConfigurePrimaryHttpMessageHandler(() => fake);

        using var services = collection.BuildServiceProvider();
        using var request = services.CreateScope();
        var health = services.GetRequiredService<UpstreamHealth>();

        await Assert.That(async () => await services.GetRequiredService<Engine.OperationEngine>().InvokeAsync(
            "trackmania.api.display_names", EngineTests.Json("""{"accountIds":["00000000-0000-0000-0000-000000000001"]}"""),
            Catalog.GameScope.Create(services.GetRequiredService<McpOptions>()), request.ServiceProvider)).ThrowsException();

        await Assert.That(fake.Requests.Count).IsEqualTo(credentialsWork ? 2 : 1);
        await Assert.That(health.Ready).IsEqualTo(credentialsWork);
    }

    [Test]
    public async Task UserAgentIsSentToEveryUpstreamService()
    {
        const string userAgent = "maniaapi-tests/1.0 (contact: test)";
        var fake = FakeHttpMessageHandler.Json("[\"Race\"]");

        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddManiaAPI(TestConfiguration.Create(new Dictionary<string, string?>
        {
            ["ManiaAPI:UserAgent"] = userAgent,
            ["ManiaAPI:NadeoAPI:Login"] = "test-login",
            ["ManiaAPI:NadeoAPI:Password"] = "test-password"
        }));
        collection.AddHttpClient("MX_Trackmania").ConfigurePrimaryHttpMessageHandler(() => fake);

        using var services = collection.BuildServiceProvider();
        using var request = services.CreateScope();

        // A client from the shared IHttpClientFactory: the exchange.
        await services.GetRequiredService<Engine.OperationEngine>().InvokeAsync("mx.maptypes", EngineTests.Json("{}"),
            Catalog.GameScope.Create(services.GetRequiredService<McpOptions>(), "trackmania2020"), request.ServiceProvider);

        var sent = fake.Request.Headers["User-Agent"];

        await Assert.That(sent).StartsWith("maniaapi-tests/1.0");
        await Assert.That(sent).Contains("(contact: test)");
        await Assert.That(sent).Contains("(TMX; Discord=bigbang1112)");

        // NadeoAPI's clients, which live in the NadeoAccount's own container.
        var nadeo = services.GetRequiredService<Catalog.NadeoAccount>().Services.GetRequiredService<NadeoAPI.NadeoServices>();

        await Assert.That(nadeo.Client.DefaultRequestHeaders.UserAgent.ToString()).StartsWith(userAgent);
    }

    [Test]
    public async Task InvalidUserAgentFailsValidation()
    {
        var result = new McpOptionsValidator().Validate(null, new McpOptions { UserAgent = "agent\r\nX-Injected: yes" });

        await Assert.That(result.Failed).IsTrue();
        await Assert.That(result.FailureMessage!).Contains("UserAgent");
    }

    private static string Base64Url(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
