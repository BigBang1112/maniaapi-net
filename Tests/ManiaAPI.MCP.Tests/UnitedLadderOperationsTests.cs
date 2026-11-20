using ManiaAPI.MCP.Catalog;
using ManiaAPI.MCP.Configuration;
using ManiaAPI.MCP.Engine;
using ManiaAPI.MCP.Hosting;
using ManiaAPI.MCP.Resources;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace ManiaAPI.MCP.Tests;

public class UnitedLadderOperationsTests
{
    [Test]
    public async Task RankingsUseUnitedLadderMetadata()
    {
        var fake = FakeHttpMessageHandler.Json("""[{"points":123.5,"id":7,"login":"player","nickname":"Player","path":"World","globalRank":2}]""");
        using var services = CreateServices(fake);
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<OperationEngine>().InvokeAsync("united.ladder.solo_rankings",
            EngineTests.Json("{}"), GameScope.Create(new McpOptions()), request.ServiceProvider);

        await Assert.That(result["data"]![0]!["points"]!.GetValue<double>()).IsEqualTo(123.5);
        await Assert.That(result["data"]![0]!["globalRank"]!.GetValue<int>()).IsEqualTo(2);
        await Assert.That(fake.Request.Uri.AbsolutePath).IsEqualTo("/solo/World/0");
    }

    [Test]
    public async Task PlayerOverloadForwardsWebServicesFlag()
    {
        var fake = FakeHttpMessageHandler.Json("""{"id":7,"login":"player","zoneId":42,"meta":{"zoneUpdate":2200000000,"stars":0}}""");
        using var services = CreateServices(fake);
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<OperationEngine>().InvokeAsync("united.ladder.player",
            EngineTests.Json("""{"login":"player","forceWebServices":true}"""), GameScope.Create(new McpOptions()), request.ServiceProvider);

        await Assert.That(fake.Request.Uri.Query).IsEqualTo("?ws=true");
        await Assert.That(result["data"]!["idZone"]!.GetValue<int>()).IsEqualTo(42);
        await Assert.That(result["data"]!["meta"]!["zoneUpdate"]!.GetValue<long>()).IsEqualTo(2200000000L);
    }

    [Test]
    public async Task HistoryBindsOptionalDates()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var services = CreateServices(fake);
        using var request = services.CreateScope();

        await services.GetRequiredService<OperationEngine>().InvokeAsync("united.ladder.player_history",
            EngineTests.Json("""{"login":"player","startDate":"2026-01-01T00:00:00Z"}"""), GameScope.Create(new McpOptions()), request.ServiceProvider);

        await Assert.That(fake.Request.Uri.Query).IsEqualTo("?startDate=1767225600000");
        await Assert.That(fake.Request.Uri.Query).DoesNotContain("endDate");
    }

    [Test]
    public async Task SnapshotPreservesEnvelopeAndNestedRankings()
    {
        var fake = FakeHttpMessageHandler.Json("""
            [{"id":1,"type":"solo","zone":"World","environment":"solo","offset_value":0,
              "snapshot_data":[{"id":7,"login":"player","points":123.5,"globalRank":2}],
              "created_at":"2026-11-19T12:00:00Z","expires_at":"2026-11-20T12:00:00Z"}]
            """);
        using var services = CreateServices(fake);
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<OperationEngine>().InvokeAsync("united.ladder.leaderboard_snapshot",
            EngineTests.Json("""{"type":"solo"}"""), GameScope.Create(new McpOptions()), request.ServiceProvider);

        await Assert.That(result["data"]![0]!["type"]!.GetValue<string>()).IsEqualTo("solo");
        await Assert.That(result["data"]![0]!["snapshot_data"]![0]!["login"]!.GetValue<string>()).IsEqualTo("player");
    }

    [Test]
    public async Task ReplayIsAvailableAsBinaryResource()
    {
        byte[] payload = [0, 255, 128, 71, 66, 88];
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(payload) });
        using var services = CreateServices(fake);
        using var request = services.CreateScope();

        var result = await services.GetRequiredService<OperationEngine>().InvokeAsync("united.ladder.replay_response",
            EngineTests.Json("""{"uid":"map_uid","playerId":7}"""), GameScope.Create(new McpOptions()), request.ServiceProvider);
        var uri = result["data"]!["resourceUri"]!.GetValue<string>();
        var resource = await services.GetRequiredService<ResourceService>().ReadAsync(uri, request.ServiceProvider, CancellationToken.None);
        var contents = (BlobResourceContents)resource.Contents.Single();

        await Assert.That(contents.DecodedData.ToArray()).IsEquivalentTo(payload);
        await Assert.That(fake.Requests.Count).IsEqualTo(2);
        await Assert.That(fake.Requests[0].Uri.AbsolutePath).IsEqualTo("/replays/map_uid/7");
    }

    private static ServiceProvider CreateServices(FakeHttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddManiaAPI(TestConfiguration.Create());
        services.AddHttpClient(nameof(UnitedLadder.UnitedLadder)).AddTypedClient(client => new UnitedLadder.UnitedLadder(client)).ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider();
    }
}
