using ManiaAPI.MCP.Accounts;
using ManiaAPI.MCP.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class LinkedTokenTests
{
    [Test]
    public async Task ConcurrentRefreshUsesOneRotatingRefreshTokenAndChecksScopes()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ManiaAPI-MCP-" + Guid.NewGuid().ToString("N"));

        try
        {
            var options = new McpOptions
            {
                AccountLinking = new AccountLinkingOptions { Enabled = true, StorePath = directory },
                TrackmaniaApi = new OAuthOptions { ClientId = "client", ClientSecret = "secret" }
            };

            var store = new LinkedAccountStore(options, new EphemeralDataProtectionProvider());
            await store.WriteAsync("caller", "trackmania", new LinkedTokens("expired", "refresh-old",
                DateTimeOffset.UtcNow.AddMinutes(-1), ["read_favorite"]), CancellationToken.None);

            var fake = FakeHttpMessageHandler.Json("""
                {"access_token":"new-token","refresh_token":"refresh-new","expires_in":3600,"scope":"read_favorite"}
                """);

            var services = new ServiceCollection();
            services.AddHttpClient("LinkedAccountTokens").ConfigurePrimaryHttpMessageHandler(() => fake);

            using var provider = services.BuildServiceProvider();
            var tokens = new LinkedAccountTokens(store, options, provider.GetRequiredService<IHttpClientFactory>(), TimeProvider.System);
            var results = await Task.WhenAll(
                tokens.GetAsync("caller", "trackmania", ["read_favorite"], CancellationToken.None),
                tokens.GetAsync("caller", "trackmania", ["read_favorite"], CancellationToken.None));

            await Assert.That(results).IsEquivalentTo(["new-token", "new-token"]);
            await Assert.That(fake.Requests.Count).IsEqualTo(1);
            await Assert.That(await store.ReadAsync("other-caller", "trackmania", CancellationToken.None)).IsNull();

            await Assert.That(async () => await tokens.GetAsync("caller", "trackmania", ["write_favorite"], CancellationToken.None))
                .Throws<ArgumentException>();
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
