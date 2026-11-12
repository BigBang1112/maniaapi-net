using System.Runtime.InteropServices;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Client;
using ManiaAPI.MCP.Resources;

namespace ManiaAPI.MCP.Tests;

public class ProtocolTests
{
    private static async Task<WebApplication> StartHttpAsync(FakeHttpMessageHandler? handler = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddManiaAPI(new ConfigurationBuilder().Build());

        if (handler is not null)
        {
            builder.Services.AddHttpClient("MX_Trackmania").ConfigurePrimaryHttpMessageHandler(() => handler);
        }

        builder.Services.AddMcpServer()
            .WithListToolsHandler((context, _) =>
                ValueTask.FromResult(context.Services!.GetRequiredService<GatewayTools>().List()))
            .WithCallToolHandler((context, cancellationToken) =>
                context.Services!.GetRequiredService<GatewayTools>().CallAsync(context.Params!, context.Services!, cancellationToken, context.Server))
            .WithListResourceTemplatesHandler((context, _) =>
                ValueTask.FromResult(context.Services!.GetRequiredService<ResourceService>().List()))
            .WithReadResourceHandler((context, token) =>
                context.Services!.GetRequiredService<ResourceService>().ReadAsync(context.Params!.Uri, context.Services!, token))
            .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless);

        var app = builder.Build();
        var options = new McpOptions { Http = new HttpOptions { ApiKeys = ["protocol-test-key"] } };
        HttpSecurity.UseApiKeys(app, options);

        app.MapMcp("/mcp");
        app.MapMcp("/mcp/trackmania2020");

        await app.StartAsync();

        return app;
    }

    [Test]
    public async Task HttpRejectsMissingAndInvalidKeys()
    {
        await using var app = await StartHttpAsync();
        using var http = app.GetTestClient();

        using var missing = await http.PostAsync("/mcp", new StringContent("{}"));
        await Assert.That(missing.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

        http.DefaultRequestHeaders.Add(HttpSecurity.ApiKeyHeader, "invalid");

        using var invalid = await http.PostAsync("/mcp", new StringContent("{}"));
        await Assert.That(invalid.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task HttpClientDiscoversScopedToolsAndSearchesWithoutSessionState()
    {
        await using var app = await StartHttpAsync();
        using var http = app.GetTestClient();
        http.DefaultRequestHeaders.Add(HttpSecurity.ApiKeyHeader, "protocol-test-key");

        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost/mcp/trackmania2020")
        }, http);

        await using var client = await McpClient.CreateAsync(transport);
        var tools = await client.ListToolsAsync();

        await Assert.That(tools.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(tools.First(x => x.Name == "maniaapi_search").Description!).Contains("Trackmania");
        await Assert.That(tools.First(x => x.Name == "maniaapi_search").Description!).DoesNotContain("TMNF,");

        var result = await client.CallToolAsync("maniaapi_search", new Dictionary<string, object?>
        {
            ["query"] = "community maps",
            ["game"] = "TM2020"
        });

        await Assert.That(result.IsError ?? false).IsFalse();
        await Assert.That(result.StructuredContent!.Value.GetRawText()).Contains("mx.search_maps");
        await Assert.That(result.StructuredContent!.Value.GetRawText()).DoesNotContain("site: Shootmania");

        var wrongScope = await client.CallToolAsync("maniaapi_search", new Dictionary<string, object?>
        {
            ["query"] = "maps",
            ["game"] = "TM2"
        });

        await Assert.That(wrongScope.IsError).IsTrue();
        await Assert.That(wrongScope.StructuredContent!.Value.GetRawText()).Contains("/mcp/maniaplanet");
    }

    [Test]
    public async Task StdioClientCompletesGatewayRoundTrip()
    {
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
            ?? Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", "..",
                OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet"));

        var testAssembly = typeof(ProtocolTests).Assembly.Location;
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = dotnet,
            Arguments =
            [
                "exec",
                "--runtimeconfig", Path.ChangeExtension(testAssembly, ".runtimeconfig.json"),
                "--depsfile", Path.ChangeExtension(testAssembly, ".deps.json"),
                typeof(MCP.Program).Assembly.Location,
                "--toolsets", "exchange"
            ],
            Name = "ManiaAPI test server"
        });

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await using var client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);

        var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
        await Assert.That(tools.Count).IsGreaterThanOrEqualTo(3);

        var result = await client.CallToolAsync("maniaapi_describe", new Dictionary<string, object?>
        {
            ["operations"] = new[] { "mx.search_maps" }
        }, cancellationToken: timeout.Token);

        await Assert.That(result.IsError ?? false).IsFalse();
        await Assert.That(result.StructuredContent!.Value.GetRawText()).Contains("inputSchema");
    }

    [Test]
    public async Task HttpResourcesUseTheSameScopeAsTools()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([1, 2, 3])
        });

        await using var app = await StartHttpAsync(fake);
        using var http = app.GetTestClient();
        http.DefaultRequestHeaders.Add(HttpSecurity.ApiKeyHeader, "protocol-test-key");

        await using var client = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost/mcp/trackmania2020")
        }, http));

        var uri = ResourceUri.Create("mx.map_gbx_response", EngineTests.Json("{\"site\":\"Trackmania\",\"mapId\":123}"));
        var resource = await client.ReadResourceAsync(uri);
        var blob = (ModelContextProtocol.Protocol.BlobResourceContents)resource.Contents.Single();

        await Assert.That(blob.DecodedData.ToArray()).IsEquivalentTo(new byte[] { 1, 2, 3 });

        var wrongSite = ResourceUri.Create("mx.map_gbx_response", EngineTests.Json("{\"site\":\"Shootmania\",\"mapId\":123}"));

        await Assert.That(async () => await client.ReadResourceAsync(wrongSite)).Throws<ModelContextProtocol.McpException>();
        await Assert.That(fake.Requests.Count).IsEqualTo(1);
    }
}
