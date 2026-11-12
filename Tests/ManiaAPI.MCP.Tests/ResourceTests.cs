using System.Text.Json;
using GBX.NET;
using GBX.NET.Engines.Game;
using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.MCP.Tests;

public class ResourceTests
{
    [Test]
    public async Task BinaryResultsReturnLinksAndResourcesDownloadAgain()
    {
        byte[] payload = [1, 2, 3, 4];
        var fake = BinaryHandler(payload);
        using var services = EngineTests.CreateServices(fake);
        using var request = services.CreateScope();
        var engine = services.GetRequiredService<OperationEngine>();

        var result = await engine.InvokeAsync("mx.map_gbx_response", EngineTests.Json("""
            {"site":"Trackmania","mapId":123}
            """), GameScope.Create(new McpOptions()), request.ServiceProvider);

        var uri = result["data"]!["resourceUri"]!.GetValue<string>();
        var resource = await services.GetRequiredService<ResourceService>().ReadAsync(uri, request.ServiceProvider, CancellationToken.None);
        var contents = (ModelContextProtocol.Protocol.BlobResourceContents)resource.Contents.Single();

        await Assert.That(contents.DecodedData.ToArray()).IsEquivalentTo(payload);
        await Assert.That(fake.Requests.Count).IsEqualTo(2);
        await Assert.That(result.ToJsonString()).DoesNotContain("AQIDBA");
    }

    [Test]
    public async Task CrossGameResourceIsRejectedBeforeDownload()
    {
        var fake = BinaryHandler([1]);
        using var services = EngineTests.CreateServices(fake);
        var engine = services.GetRequiredService<OperationEngine>();
        var capture = new BinaryCapture();

        await Assert.That(async () => await engine.InvokeAsync("mx.map_gbx_response", EngineTests.Json("""
            {"site":"Shootmania","mapId":123}
            """), GameScope.Create(new McpOptions(), "trackmania2020"), services, capture: capture))
            .Throws<ArgumentException>();

        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task BoundedDownloadRejectsOversizedStream()
    {
        using var stream = new MemoryStream(new byte[100]);

        await Assert.That(async () => await BoundedRead.AllAsync(stream, 10, CancellationToken.None))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task GbxOperationsReturnHeadersAndNeverTheObjectGraph()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Test.Map.Gbx");
        var payload = await File.ReadAllBytesAsync(path);
        var expected = Gbx.ParseHeaderNode<CGameCtnChallenge>(path);

        using var services = EngineTests.CreateServices(BinaryHandler(payload));
        using var request = services.CreateScope();
        var result = await services.GetRequiredService<OperationEngine>().InvokeAsync("mx.map_gbx", EngineTests.Json("""
            {"site":"Trackmania","mapId":123}
            """), GameScope.Create(new McpOptions()), request.ServiceProvider);

        await Assert.That(result["data"]!["uid"]!.GetValue<string>()).IsEqualTo(expected.MapUid);
        await Assert.That(result["data"]!["resourceUri"]!.GetValue<string>()).StartsWith("maniaapi://");
        await Assert.That(result.ToJsonString()).DoesNotContain("chunks");
    }

    private static FakeHttpMessageHandler BinaryHandler(byte[] payload)
    {
        return new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(payload)
        });
    }
}
