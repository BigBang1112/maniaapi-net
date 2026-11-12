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
}
