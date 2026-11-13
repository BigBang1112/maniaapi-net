using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ManiaAPI.MCP.Accounts;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Client;

namespace ManiaAPI.MCP.Tests;

public class AccountLinkingTests
{
    [Test]
    [Arguments("trackmania")]
    [Arguments("maniaplanet")]
    public async Task BrowserConsentLinksOnlyTheInitiatingApiKey(string provider)
    {
        var directory = Path.Combine(Path.GetTempPath(), "ManiaAPI-MCP-" + Guid.NewGuid().ToString("N"));
        var oauth = new FakeHttpMessageHandler(request => request.RequestUri!.AbsolutePath.EndsWith("access_token", StringComparison.Ordinal)
            ? FakeResponse.Json("""
                {"access_token":"player-token","refresh_token":"refresh-token","expires_in":3600,"token_type":"Bearer","scope":"basic clubs read_favorite write_favorite"}
                """)
            : FakeResponse.Json("""
                {"accountId":"00000000-0000-0000-0000-000000000001","displayName":"Player","login":"player","nickname":"Player"}
                """));

        var player = FakeHttpMessageHandler.Json("""
            {"accountId":"00000000-0000-0000-0000-000000000001","displayName":"Player","login":"player","nickname":"Player"}
            """);

        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddManiaAPI(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ManiaAPI:AccountLinking:Enabled"] = "true",
                ["ManiaAPI:AccountLinking:PublicOrigin"] = "https://localhost",
                ["ManiaAPI:AccountLinking:StorePath"] = Path.Combine(directory, "tokens"),
                ["ManiaAPI:AccountLinking:DataProtectionKeyPath"] = Path.Combine(directory, "keys"),
                ["ManiaAPI:Http:ApiKeys:0"] = "caller-a",
                ["ManiaAPI:Http:ApiKeys:1"] = "caller-b",
                ["ManiaAPI:TrackmaniaAPI:ClientId"] = "client-id",
                ["ManiaAPI:TrackmaniaAPI:ClientSecret"] = "client-secret",
                ["ManiaAPI:ManiaPlanet:ClientId"] = "client-id",
                ["ManiaAPI:ManiaPlanet:ClientSecret"] = "client-secret"
            }).Build());

            builder.Services.PostConfigure<ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Authentication.TrackmaniaAuthenticationOptions>(
                "LinkTrackmania", settings => settings.Backchannel = oauth.CreateClient());
            builder.Services.PostConfigure<ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication.ManiaPlanetAuthenticationOptions>(
                "LinkManiaplanet", settings => settings.Backchannel = oauth.CreateClient());
            builder.Services.AddHttpClient("LinkedPlayer").ConfigurePrimaryHttpMessageHandler(() => player);

            builder.Services.AddMcpServer()
                .WithListToolsHandler((context, _) => ValueTask.FromResult(context.Services!.GetRequiredService<GatewayTools>().List()))
                .WithCallToolHandler((context, token) => context.Services!.GetRequiredService<GatewayTools>()
                    .CallAsync(context.Params!, context.Services!, token, context.Server))
                .WithHttpTransport(settings => settings.SessionMode = HttpServerSessionMode.Stateless);

            await using var app = builder.Build();
            var options = app.Services.GetRequiredService<McpOptions>();
            HttpSecurity.UseApiKeys(app, options);
            AccountLinking.Map(app, options);
            app.MapMcp("/mcp");
            await app.StartAsync();

            using var api = app.GetTestClient();
            using var missingKey = await api.PostAsync("/account-links/" + provider, null);
            await Assert.That(missingKey.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

            api.DefaultRequestHeaders.Add(HttpSecurity.ApiKeyHeader, "caller-a");
            using var initiated = await api.PostAsync("/account-links/" + provider, null);
            using var body = JsonDocument.Parse(await initiated.Content.ReadAsStringAsync());
            var startUrl = body.RootElement.GetProperty("url").GetString()!;

            using var browser = app.GetTestClient();
            using var start = await browser.GetAsync(startUrl);
            await Assert.That(start.StatusCode).IsEqualTo(HttpStatusCode.Redirect);

            var authorization = QueryHelpers.ParseQuery(start.Headers.Location!.Query);
            await Assert.That(authorization.ContainsKey("code_challenge")).IsTrue();

            var cookies = string.Join("; ", start.Headers.GetValues("Set-Cookie").Select(value => value.Split(';')[0]));
            browser.DefaultRequestHeaders.Add("Cookie", cookies);
            var callback = provider == "trackmania" ? "/signin-trackmania" : "/signin-maniaplanet";

            using var completed = await browser.GetAsync(callback + "?code=test-code&state=" + Uri.EscapeDataString(authorization["state"]!));
            await Assert.That(completed.StatusCode).IsEqualTo(HttpStatusCode.OK);

            var operation = provider == "trackmania" ? "trackmania.api.user" : "maniaplanet.web.player";
            await using var clientA = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri("http://localhost/mcp")
            }, api));

            var result = await clientA.CallToolAsync("maniaapi_invoke", new Dictionary<string, object?>
            {
                ["operation"] = operation,
                ["arguments"] = new Dictionary<string, object?>()
            });

            await Assert.That(result.IsError ?? false).IsFalse();
            await Assert.That(result.StructuredContent!.Value.GetRawText()).DoesNotContain("player-token");
            await Assert.That(player.Request.Authorization!.Parameter).IsEqualTo("player-token");

            using var otherApi = app.GetTestClient();
            otherApi.DefaultRequestHeaders.Add(HttpSecurity.ApiKeyHeader, "caller-b");
            await using var clientB = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri("http://localhost/mcp")
            }, otherApi));

            var other = await clientB.CallToolAsync("maniaapi_invoke", new Dictionary<string, object?>
            {
                ["operation"] = operation,
                ["arguments"] = new Dictionary<string, object?>()
            });

            await Assert.That(other.IsError).IsTrue();
            await Assert.That(player.Requests.Count).IsEqualTo(1);

            var encrypted = await File.ReadAllBytesAsync(Directory.GetFiles(Path.Combine(directory, "tokens"), "*.tokens").Single());
            await Assert.That(Encoding.UTF8.GetString(encrypted)).DoesNotContain("player-token");

            using var unlinked = await api.DeleteAsync("/account-links/" + provider);
            await Assert.That(unlinked.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
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
