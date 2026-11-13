using System.Text.Json.Nodes;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace ManiaAPI.MCP.Tests;

public class WriteTests
{
    [Test]
    public async Task WritesStayHiddenAndRejectedByDefault()
    {
        using var services = EngineTests.CreateServices();
        var options = services.GetRequiredService<McpOptions>();
        var engine = services.GetRequiredService<OperationEngine>();

        await Assert.That(engine.Available(GameScope.Create(options)).All(operation => operation.ReadOnly)).IsTrue();
        await Assert.That(async () => await engine.InvokeAsync("trackmania.api.add_user_favorite_map",
            EngineTests.Json("{\"mapUid\":\"map\"}"), GameScope.Create(options), services, confirmed: true))
            .Throws<ArgumentException>();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task MrtrOnlyInvokesAfterApproval(bool approve)
    {
        var calls = 0;
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddManiaAPI(TestConfiguration.Create(new Dictionary<string, string?>
        {
            ["ManiaAPI:AllowWrites"] = "true",
            ["ManiaAPI:Toolsets:0"] = "exchange",
            ["ManiaAPI:Http:ApiKeys:0"] = "write-test"
        }));

        builder.Services.AddSingleton(new[] { WriteOperation(() => Interlocked.Increment(ref calls)) });
        builder.Services.AddMcpServer()
            .WithListToolsHandler((context, _) => ValueTask.FromResult(context.Services!.GetRequiredService<GatewayTools>().List()))
            .WithCallToolHandler((context, token) => context.Services!.GetRequiredService<GatewayTools>()
                .CallAsync(context.Params!, context.Services!, token, context.Server))
            .WithHttpTransport(settings => settings.SessionMode = HttpServerSessionMode.Stateless);

        await using var app = builder.Build();
        HttpSecurity.UseApiKeys(app, app.Services.GetRequiredService<McpOptions>());
        app.MapMcp("/mcp");
        await app.StartAsync();

        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add(HttpSecurity.ApiKeyHeader, "write-test");
        var prompts = 0;

        await using var client = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(address + "/mcp")
        }, http), new McpClientOptions
        {
            Handlers = new McpClientHandlers
            {
                ElicitationHandler = (request, token) =>
                {
                    Interlocked.Increment(ref prompts);

                    return ValueTask.FromResult(new ElicitResult
                    {
                        Action = approve ? "accept" : "decline",
                        Content = new Dictionary<string, System.Text.Json.JsonElement>
                        {
                            ["confirm"] = EngineTests.Json(approve ? "true" : "false")
                        }
                    });
                }
            }
        });

        var result = await client.CallToolAsync("maniaapi_invoke", new Dictionary<string, object?>
        {
            ["operation"] = "test.write",
            ["arguments"] = new Dictionary<string, object?>(),
            ["confirm"] = true
        });

        await Assert.That(prompts).IsEqualTo(1);
        await Assert.That(calls).IsEqualTo(approve ? 1 : 0);
        await Assert.That(result.IsError ?? false).IsEqualTo(!approve);
    }

    [Test]
    public async Task ExplicitConfirmationFallbackRejectsUnconfirmedWrites()
    {
        using var services = EngineTests.CreateServices();
        var confirmation = services.GetRequiredService<WriteConfirmation>();
        var operation = WriteOperation(() => { });
        var request = new CallToolRequestParams { Name = "maniaapi_invoke" };

        await Assert.That(() => confirmation.Authorize(operation, EngineTests.Json("{}"),
            "exchange", "stdio", false, request, null)).Throws<ArgumentException>();

        await Assert.That(confirmation.Authorize(operation, EngineTests.Json("{}"),
            "exchange", "stdio", true, request, null)).IsTrue();
    }

    private static Operation WriteOperation(Action invoke)
    {
        return new Operation("test.write", "exchange", "A fake mutation used to verify confirmation.", "none", null, false, false,
        [
            new OperationOverload(CatalogMetadata.Input(), "", new JsonObject(), (_, _, _) =>
            {
                invoke();

                return Task.FromResult<JsonNode?>(new JsonObject { ["completed"] = true });
            })
        ]) { ReadOnly = false };
    }
}
