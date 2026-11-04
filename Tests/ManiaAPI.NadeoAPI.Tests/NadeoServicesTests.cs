namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoServicesTests
{
    private const string BaseUrl = "https://prod.trackmania.core.nadeo.online";

    private static readonly Guid AccountA = Guid.Parse("4a983dc3-a938-4564-8a1c-8be20779f069");
    private static readonly Guid AccountB = Guid.Parse("5dda004f-add9-43ad-b9cd-7a90101aa8f2");
    private static readonly Guid MapA = Guid.Parse("ed53486f-a80d-419b-b073-54bb8944d502");
    private static readonly Guid MapB = Guid.Parse("e8a02ba9-f418-463d-8a83-ac5fc465ca8a");

    private static string MapRecordJson(Guid accountId, Guid mapId, int time) => $$"""
        {
          "accountId": "{{accountId}}",
          "filename": "Replays/record.Replay.Gbx",
          "gameMode": "TimeAttack",
          "gameModeCustomData": "",
          "mapId": "{{mapId}}",
          "mapRecordId": "{{Guid.NewGuid()}}",
          "medal": 3,
          "recordScore": { "time": {{time}}, "score": 0, "respawnCount": 2 },
          "removed": false,
          "scopeId": null,
          "scopeType": "PersonalBest",
          "timestamp": "2024-05-01T12:00:00+00:00",
          "url": "https://example.com/record.Replay.Gbx"
        }
        """;

    private const string MapInfoJson = """
        {
          "author": "4a983dc3-a938-4564-8a1c-8be20779f069",
          "authorScore": 45123,
          "bronzeScore": 68000,
          "collectionName": "Stadium",
          "filename": "Test.Map.Gbx",
          "goldScore": 48000,
          "isPlayable": true,
          "mapId": "ed53486f-a80d-419b-b073-54bb8944d502",
          "mapStyle": "",
          "mapType": "TrackMania\\TM_Race",
          "mapUid": "UidOfTheMap",
          "name": "$sTest map",
          "silverScore": 55000,
          "submitter": "4a983dc3-a938-4564-8a1c-8be20779f069",
          "timestamp": "2024-05-01T12:00:00+00:00",
          "fileUrl": "https://example.com/map.Map.Gbx",
          "thumbnailUrl": "https://example.com/map.jpg"
        }
        """;

    [Test]
    public async Task GetMapRecordsAsync_SendsOneRequestPerMapAndCombinesResults()
    {
        var fake = new FakeHttpMessageHandler(request =>
        {
            var mapId = request.RequestUri!.Query.Contains(MapA.ToString()) ? MapA : MapB;
            return FakeResponse.Json($"[{MapRecordJson(AccountA, mapId, 45000)}]");
        });
        using var ns = new NadeoServices(fake.CreateClient());

        var records = await ns.GetMapRecordsAsync([AccountA, AccountB], [MapA, MapB]);

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo(
        [
            $"{BaseUrl}/v2/mapRecords/by-account/?accountIdList={AccountA},{AccountB}&mapId={MapA}",
            $"{BaseUrl}/v2/mapRecords/by-account/?accountIdList={AccountA},{AccountB}&mapId={MapB}"
        ]);
        await Assert.That(records.Select(x => x.MapId)).IsEquivalentTo([MapA, MapB]);
    }

    [Test]
    public async Task GetMapRecordsAsync_WithSeasonAndGameMode_AddsQueryParameters()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.GetMapRecordsAsync([AccountA], MapA, seasonId: "season", gamemode: "TimeAttack");

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/v2/mapRecords/by-account/?accountIdList={AccountA}&mapId={MapA}&seasonId=season&gameMode=TimeAttack");
    }

    [Test]
    public async Task GetMapRecordsByIdsAsync_ParsesRecord()
    {
        var fake = FakeHttpMessageHandler.Json($"[{MapRecordJson(AccountA, MapA, 45123)}]");
        using var ns = new NadeoServices(fake.CreateClient());

        var records = await ns.GetMapRecordsByIdsAsync(MapA);

        var record = records.Single();
        await Assert.That(record.AccountId).IsEqualTo(AccountA);
        await Assert.That(record.Medals).IsEqualTo(3);
        await Assert.That(record.RecordScore.Time.TotalMilliseconds).IsEqualTo(45123);
        await Assert.That(record.RecordScore.RespawnCount).IsEqualTo(2);
        await Assert.That(record.ScopeId).IsNull();
        await Assert.That(record.FileName).IsEqualTo("Replays/record.Replay.Gbx");
    }

    [Test]
    public async Task GetMapInfoAsync_ByUid_ParsesMapInfo()
    {
        var fake = FakeHttpMessageHandler.Json($"[{MapInfoJson}]");
        using var ns = new NadeoServices(fake.CreateClient());

        var map = await ns.GetMapInfoAsync("UidOfTheMap");

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/maps/by-uid/?mapUidList=UidOfTheMap");
        await Assert.That(map).IsNotNull();
        await Assert.That(map!.MapId).IsEqualTo(MapA);
        await Assert.That(map.AuthorScore.TotalMilliseconds).IsEqualTo(45123);
        await Assert.That(map.MapType).IsEqualTo("TrackMania\\TM_Race");
        await Assert.That(map.FileName).IsEqualTo("Test.Map.Gbx");
    }

    [Test]
    public async Task GetMapInfoAsync_ById_ReturnsNullWhenNotFound()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ns = new NadeoServices(fake.CreateClient());

        var map = await ns.GetMapInfoAsync(MapA);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/maps/by-id/?mapIdList={MapA}");
        await Assert.That(map).IsNull();
    }

    [Test]
    public async Task GetZonesAsync_ParsesZones()
    {
        var fake = FakeHttpMessageHandler.Json("""
            [
              { "icon": "world.dds", "name": "World", "parentId": null, "timestamp": "2020-07-01T00:00:00+00:00", "zoneId": "301e1b69-7e13-11e8-8060-e284abfd2bc4" },
              { "icon": "europe.dds", "name": "Europe", "parentId": "301e1b69-7e13-11e8-8060-e284abfd2bc4", "timestamp": "2020-07-01T00:00:00+00:00", "zoneId": "301e2106-7e13-11e8-8060-e284abfd2bc4" }
            ]
            """);
        using var ns = new NadeoServices(fake.CreateClient());

        var zones = await ns.GetZonesAsync();

        await Assert.That(zones.Select(x => x.Name)).IsEquivalentTo(["World", "Europe"]);
        await Assert.That(zones[0].ParentId).IsNull();
        await Assert.That(zones[1].ParentId).IsEqualTo(zones[0].ZoneId);
    }

    [Test]
    public async Task GetApiRoutesAsync_AddsUsage()
    {
        var fake = FakeHttpMessageHandler.Json("""{"getZones":{"method":"GET","path":"/zones"}}""");
        using var ns = new NadeoServices(fake.CreateClient());

        var routes = await ns.GetApiRoutesAsync(ApiUsage.Server);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/api/routes?usage=Server");
        await Assert.That(routes["getZones"].Path).IsEqualTo("/zones");
    }

    [Test]
    [Arguments(null, "count=50&offset=10")]
    [Arguments(5, "count=50&offset=10&trophyType=5")]
    public async Task GetPlayerTrophyHistoryAsync_AddsTrophyTypeOnlyWhenSet(int? trophyType, string expectedQuery)
    {
        var fake = FakeHttpMessageHandler.Json("""{"count":0,"data":[],"offset":10,"totalCount":0}""");
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.GetPlayerTrophyHistoryAsync(AccountA, count: 50, offset: 10, trophyType: trophyType);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/accounts/{AccountA}/trophies?{expectedQuery}");
    }

    [Test]
    public async Task AddFavoriteMapAsync_PostsMapUid()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.AddFavoriteMapAsync("UidOfTheMap");

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/maps/favorites");
        await Assert.That(fake.Request.Body).IsEqualTo("""{"mapUid":"UidOfTheMap"}""");
    }

    [Test]
    public async Task RemoveFavoriteMapAsync_SendsDelete()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.RemoveFavoriteMapAsync("UidOfTheMap");

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Delete);
        await Assert.That(fake.Request.Body).IsEqualTo("""{"mapUid":"UidOfTheMap"}""");
    }

    [Test]
    public async Task AddFavoriteMapAsync_EmptyUid_ThrowsWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<ArgumentException>(() => ns.AddFavoriteMapAsync(""));
        await Assert.That(fake.Requests).IsEmpty();
    }
}
