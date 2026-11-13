namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class ManiaPlanetChannelRecordAPITests
{
    private const string RecordsUrl = "https://records.prod.live.maniaplanet.com";
    private const string MapUid = "9W_9qjV56i67CWpYDclFyOhz5g";

    // Shapes as returned by the live API
    private const string RankingJson = """
        {"1":{"userLogin":"kripke","userNickname":"$22aPa$33atr$44aic$55ak","rank":51,"score":17801},
         "2":{"userLogin":"tona.","userNickname":"$i$fe0NADEO","rank":52,"score":17878}}
        """;

    [Test]
    public async Task GetPlayerRecordAsync_RequestsPlayerRoute()
    {
        var fake = FakeHttpMessageHandler.Json("""{"length":1821}""");
        var api = CreateApi(fake);

        await api.GetPlayerRecordAsync(MapUid, "tona.");

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Get);
        await Assert.That(fake.Request.Url).IsEqualTo($"{RecordsUrl}/leaderboard/{MapUid}/user/tona./channel");
    }

    [Test]
    public async Task GetPlayerRecordAsync_Ranked_ReturnsRecordAndCount()
    {
        var fake = FakeHttpMessageHandler.Json("""{"userLogin":"lbt76240","userNickname":"$00FLBT","rank":1135,"score":20722,"length":1821}""");
        var api = CreateApi(fake);

        var result = await api.GetPlayerRecordAsync(MapUid, "lbt76240");

        await Assert.That(result!.Record).IsEqualTo(new ChannelRecord("lbt76240", "$00FLBT", 1135, 20722));
        await Assert.That(result.RecordCount).IsEqualTo(1821);
    }

    [Test]
    public async Task GetPlayerRecordAsync_Unranked_ReturnsCountWithoutRecord()
    {
        var fake = FakeHttpMessageHandler.Json("""{"length":1821}""");
        var api = CreateApi(fake);

        var result = await api.GetPlayerRecordAsync(MapUid, "bigbang1112");

        await Assert.That(result!.Record).IsNull();
        await Assert.That(result.RecordCount).IsEqualTo(1821);
    }

    [Test]
    public async Task GetPlayerRecordAsync_UnknownMap_ReturnsNull()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NoContent));
        var api = CreateApi(fake);

        await Assert.That(await api.GetPlayerRecordAsync("unknown", "tona.")).IsNull();
    }

    [Test]
    public async Task GetPlayerRecordAsync_ReadsNumbersSentAsStrings()
    {
        // The API documentation shows numbers as strings
        var fake = FakeHttpMessageHandler.Json("""{"userLogin":"test","userNickname":"test","rank":"1","score":"5000","length":"1"}""");
        var api = CreateApi(fake);

        var result = await api.GetPlayerRecordAsync(MapUid, "test");

        await Assert.That(result!.Record!.Score).IsEqualTo(5000);
        await Assert.That(result.RecordCount).IsEqualTo(1);
    }

    [Test]
    public async Task GetLeaderboardAsync_PostsMapPlayersAndType()
    {
        var fake = FakeHttpMessageHandler.Json($$$"""{"ranking":{{{RankingJson}}},"length":1821,"listPlayer":{"lbt76240":1135}}""");
        var api = CreateApi(fake);

        await api.GetLeaderboardAsync(MapUid, ["lbt76240", "bigbang1112"]);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{RecordsUrl}/leaderboard/get");
        await Assert.That(fake.Request.Body).IsEqualTo($$"""{"mapUid":"{{MapUid}}","listPlayer":["lbt76240","bigbang1112"],"type":"channel"}""");
    }

    [Test]
    public async Task GetLeaderboardAsync_WithoutPlayers_SendsEmptyList()
    {
        // The API answers 400 when the list is missing
        var fake = FakeHttpMessageHandler.Json($$"""{"ranking":{{RankingJson}},"length":1821}""");
        var api = CreateApi(fake);

        var result = await api.GetLeaderboardAsync(MapUid);

        await Assert.That(fake.Request.Body).Contains("\"listPlayer\":[]");
        await Assert.That(result!.PlayerRanks).IsEmpty();
    }

    [Test]
    public async Task GetLeaderboardAsync_ReturnsTopCountAndPlayerRanks()
    {
        var fake = FakeHttpMessageHandler.Json($$$"""{"ranking":{{{RankingJson}}},"length":1821,"listPlayer":{"lbt76240":1135}}""");
        var api = CreateApi(fake);

        var result = await api.GetLeaderboardAsync(MapUid, ["lbt76240", "bigbang1112"]);

        await Assert.That(result!.Top.Select(x => x.UserLogin)).IsEquivalentTo(["kripke", "tona."]);
        await Assert.That(result.RecordCount).IsEqualTo(1821);
        await Assert.That(result.PlayerRanks["lbt76240"]).IsEqualTo(1135);
        await Assert.That(result.PlayerRanks.ContainsKey("bigbang1112")).IsFalse();
    }

    [Test]
    public async Task GetLeaderboardAsync_NoLeaderboard_ReturnsNull()
    {
        var fake = FakeHttpMessageHandler.Text("No leaderboard for this map", statusCode: HttpStatusCode.BadRequest);
        var api = CreateApi(fake);

        await Assert.That(await api.GetLeaderboardAsync("unknown")).IsNull();
    }

    [Test]
    public async Task GetRecordsAsync_PostsOneBasedInclusiveRange()
    {
        var fake = FakeHttpMessageHandler.Json($$"""{"ranking":{{RankingJson}}}""");
        var api = CreateApi(fake);

        await api.GetRecordsAsync(MapUid, offset: 50, length: 101);

        await Assert.That(fake.Request.Url).IsEqualTo($"{RecordsUrl}/leaderboard/getrange");
        await Assert.That(fake.Request.Body).IsEqualTo($$"""{"mapUid":"{{MapUid}}","rangeStart":51,"rangeEnd":151,"type":"channel"}""");
    }

    [Test]
    public async Task GetRecordsAsync_ReturnsRecordsOrderedByPosition()
    {
        // Keys are positions in the response, not ranks, and the order of keys isn't guaranteed
        var fake = FakeHttpMessageHandler.Json("""
            {"ranking":{"2":{"userLogin":"b","userNickname":"b","rank":52,"score":2},"1":{"userLogin":"a","userNickname":"a","rank":51,"score":1}}}
            """);
        var api = CreateApi(fake);

        var records = await api.GetRecordsAsync(MapUid, offset: 50, length: 2);

        await Assert.That(records.Select(x => x.Rank)).IsEquivalentTo([51, 52]);
    }

    [Test]
    public async Task GetRecordsAsync_PastLastRecord_ReturnsEmpty()
    {
        var fake = FakeHttpMessageHandler.Text("No leaderboard for this map", statusCode: HttpStatusCode.BadRequest);
        var api = CreateApi(fake);

        await Assert.That(await api.GetRecordsAsync(MapUid, offset: 5000)).IsEmpty();
    }

    [Test]
    public async Task GetRecordsAsync_OtherBadRequest_Throws()
    {
        var fake = FakeHttpMessageHandler.Text("Bad Range", statusCode: HttpStatusCode.BadRequest);
        var api = CreateApi(fake);

        await Assert.ThrowsAsync<HttpRequestException>(() => api.GetRecordsAsync(MapUid));
    }

    [Test]
    [Arguments(-1, 10)]
    [Arguments(0, 0)]
    [Arguments(0, 102)]
    public async Task GetRecordsAsync_RangeOutsideLimits_ThrowsWithoutRequest(int offset, int length)
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        var api = CreateApi(fake);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => api.GetRecordsAsync(MapUid, offset, length));
        await Assert.That(fake.Requests).IsEmpty();
    }

    private static ManiaPlanetChannelRecordAPI CreateApi(FakeHttpMessageHandler fake) => new(fake.CreateClient(ManiaPlanetChannelRecordAPI.BaseAddress));
}
