using System.Security.Cryptography;
using System.Text;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class ConfigurationTests
{
    [Test]
    public async Task MissingUpstreamCredentialsAreValid()
    {
        var result = new McpOptionsValidator().Validate(null, new McpOptions());

        await Assert.That(result.Succeeded).IsTrue();
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
        services.AddManiaAPI(new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<McpOptions>().Toolsets).IsEquivalentTo(McpOptions.SupportedToolsets);
    }

    [Test]
    public async Task ConfiguredArraysReplaceDefaults()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ManiaAPI:Toolsets:0"] = "exchange",
            ["ManiaAPI:Exchange:Sites:0"] = "TMNF"
        }).Build();

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
    public async Task CommandLineToolsetsReplaceConfiguredArray()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ManiaAPI:Toolsets:0"] = "trackmania2020",
            ["ManiaAPI:Toolsets:1"] = "exchange"
        }).Build();

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
        collection.AddManiaAPI(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ManiaAPI:TrackmaniaAPI:ClientId"] = "test-client",
            ["ManiaAPI:TrackmaniaAPI:ClientSecret"] = "test-secret"
        }).Build());
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

    private static string Base64Url(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
