using System.Globalization;
using System.Text.Json;

namespace ManiaAPI.UnitedLadder.Tests;

public class UnitedLadderEndpointsTests
{
    private const string RankingJson = """[{"points":123456.75,"id":7,"login":"player","nickname":"$f00Player","path":"World|France","globalRank":2}]""";

    [Test]
    [Arguments("solo", "solo/World/0")]
    [Arguments("multi", "multi/World/Stadium/2")]
    [Arguments("track", "tracks/World/map_uid/3")]
    [Arguments("maps", "solomaps/player/4")]
    [Arguments("breakdown", "solobreakdown/player")]
    [Arguments("login", "checklogin/player")]
    [Arguments("history", "history/player")]
    [Arguments("progression", "progression/player?field=skillpoints")]
    [Arguments("snapshot", "snapshot/solo/World")]
    [Arguments("replay", "replays/map_uid/7")]
    [Arguments("player", "player/player?ws=true")]
    public async Task EndpointsUseDocumentedRoutes(string operation, string path)
    {
        var fake = FakeHttpMessageHandler.Json(IsArray(operation) ? "[]" : "{}");
        using var ul = new UnitedLadder(fake.CreateClient());

        await InvokeAsync(ul, operation);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Get);
        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo($"{UnitedLadder.BaseAddress}/{path}");
    }

    [Test]
    public async Task PlayerIncludesStatisticsAndAcceptsDocumentedZoneId()
    {
        var fake = FakeHttpMessageHandler.Json("""
            {"id":7,"login":"player","nickname":"Player","united":true,"path":"World|France","zoneId":42,"legacy":false,
             "solo":{"environment":"solo","points":123456.75,"ranks":[{"rank":2,"zoneId":42,"zonePath":"World|France"}]},
             "multiplayer":{"Stadium":{"points":123.5,"wins":10,"losses":3,"ranks":[{"rank":1,"zoneId":42,"zonePath":"World|France"}]}},
             "tags":[],"meta":{"zoneUpdate":2200000000,"stars":0}}
            """);
        using var ul = new UnitedLadder(fake.CreateClient());

        var player = await ul.GetPlayerAsync("player");

        await Assert.That(player.IdZone).IsEqualTo(42);
        await Assert.That(player.ZoneId).IsEqualTo((int?)42);
        await Assert.That(player.Solo!.Points).IsEqualTo(123456.75);
        await Assert.That(player.Solo.Ranks.Single().ZonePath).IsEqualTo("World|France");
        await Assert.That(player.Multiplayer!["Stadium"].Points).IsEqualTo(123.5);
        await Assert.That(player.Multiplayer["Stadium"].Wins).IsEqualTo(10);
        await Assert.That(player.Meta!.ZoneUpdate).IsEqualTo(2200000000L);
    }

    [Test]
    public async Task RankingsPreserveFractionalPointsAndGlobalRank()
    {
        var fake = FakeHttpMessageHandler.Json(RankingJson);
        using var ul = new UnitedLadder(fake.CreateClient());

        var rankings = await ul.GetSoloRankingsAsync();

        await Assert.That(rankings.Single().Points).IsEqualTo(123456.75);
        await Assert.That(rankings[0].GlobalRank).IsEqualTo(2);
        await Assert.That(rankings[0].Nickname).IsEqualTo("$f00Player");
    }

    [Test]
    public async Task TrackLeaderboardIncludesRecordsAndGhostLinks()
    {
        var fake = FakeHttpMessageHandler.Json("""
            {"uid":"map_uid","name":"Map","author":"author","envi":"Stadium","gameMode":"Race","recordCount":100,
             "records":[{"points":45678,"id":7,"login":"player","nickname":"Player","path":"World","globalRank":1,"skillpoints":2200000000,"ghost":"https://example.com/ghost.Replay.Gbx"}]}
            """);
        using var ul = new UnitedLadder(fake.CreateClient());

        var leaderboard = await ul.GetTrackLeaderboardAsync("map_uid");

        await Assert.That(leaderboard.Envi).IsEqualTo("Stadium");
        await Assert.That(leaderboard.RecordCount).IsEqualTo(100);
        await Assert.That(leaderboard.Records.Single().Points).IsEqualTo(45678);
        await Assert.That(leaderboard.Records[0].Skillpoints).IsEqualTo(2200000000L);
        await Assert.That(leaderboard.Records[0].Ghost).IsEqualTo("https://example.com/ghost.Replay.Gbx");
    }

    [Test]
    [Arguments("123.5")]
    [Arguments("\"123.5\"")]
    public async Task PlayerMapsIncludeEnvironmentAndPoints(string pointsJson)
    {
        var fake = FakeHttpMessageHandler.Json($$"""[{"uid":"map_uid","name":"Map","points":{{pointsJson}},"envi":"Snow"}]""");
        using var ul = new UnitedLadder(fake.CreateClient());

        var maps = await ul.GetPlayerMapsAsync("player");

        await Assert.That(maps.Single()).IsEqualTo(new MapPoints("map_uid", "Map", 123.5, "Snow"));
    }

    [Test]
    public async Task SoloBreakdownIncludesEnvironmentCampaignAndSummary()
    {
        var fake = FakeHttpMessageHandler.Json("""
            {"environments":{"Stadium":{"totalPoints":2200000000,"mapCount":2,"averagePoints":1100000000,
              "topMaps":[{"uid":"map_uid","name":"Map","points":123.5,"envi":"Stadium"}],"allMaps":[]}},
             "campaigns":{"United":{"totalPoints":2200000000,"mapCount":2,"averagePoints":1100000000,
              "environments":["Stadium"],"environmentCount":1,"topMaps":[],"allMaps":[]}},
             "summary":{"totalPoints":2200000000.5,"totalMaps":2,"environmentCount":1}}
            """);
        using var ul = new UnitedLadder(fake.CreateClient());

        var breakdown = await ul.GetPlayerSoloBreakdownAsync("player");

        await Assert.That(breakdown.Environments["Stadium"].TotalPoints).IsEqualTo(2200000000L);
        await Assert.That(breakdown.Environments["Stadium"].TopMaps.Single().Points).IsEqualTo(123.5);
        await Assert.That(breakdown.Campaigns["United"].Environments.Single()).IsEqualTo("Stadium");
        await Assert.That(breakdown.Summary.TotalPoints).IsEqualTo(2200000000.5);
    }

    [Test]
    public async Task CheckLoginReturnsRegistrationAndAccountType()
    {
        var fake = FakeHttpMessageHandler.Json("""{"registered":true,"isUnited":false}""");
        using var ul = new UnitedLadder(fake.CreateClient());

        var login = await ul.CheckLoginAsync("player");

        await Assert.That(login).IsEqualTo(new LoginData(true, false));
    }

    [Test]
    public async Task HistoryReadsSnakeCaseFieldsAndNestedRankings()
    {
        var fake = FakeHttpMessageHandler.Json("""
            [{"data_date":"2026-11-01T12:00:00Z","full_data":{"id":7,"login":"player","zoneId":42},
              "summary_data":{"path":"World|France","ranks":{"solo":[{"rank":2,"zone":42,"zone_path":"World|France"}],
                "multiplayer":{"Stadium":[{"rank":1,"zone":42,"zone_path":"World|France"}]}},
                "stars":0,"united":true,"id_zone":42,"nickname":"Old name","tag_count":3,"environment":"solo",
                "skillpoints":123456.75,"zone_update":2200000000,"multiplayer_points":{"Stadium":123.5}},
              "is_permanent":true,"created_at":"2026-11-01T12:01:00Z","updated_at":"2026-11-02T12:00:00Z"}]
            """);
        using var ul = new UnitedLadder(fake.CreateClient());

        var history = (await ul.GetPlayerHistoryAsync("player")).Single();

        await Assert.That(history.DataDate).IsEqualTo(DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture));
        await Assert.That(history.FullData!.IdZone).IsEqualTo(42);
        await Assert.That(history.IsPermanent).IsTrue();
        await Assert.That(history.SummaryData!.IdZone).IsEqualTo(42);
        await Assert.That(history.SummaryData.TagCount).IsEqualTo(3);
        await Assert.That(history.SummaryData.Ranks.Solo.Single().ZonePath).IsEqualTo("World|France");
        await Assert.That(history.SummaryData.Ranks.Multiplayer["Stadium"].Single().Rank).IsEqualTo(1);
        await Assert.That(history.SummaryData.MultiplayerPoints["Stadium"]).IsEqualTo(123.5);
        await Assert.That(history.SummaryData.ZoneUpdate).IsEqualTo(2200000000L);
    }

    [Test]
    public async Task ProgressionPreservesNicknameValues()
    {
        var fake = FakeHttpMessageHandler.Json("""[{"date":"2026-11-01T12:00:00Z","value":"$f00New name"}]""");
        using var ul = new UnitedLadder(fake.CreateClient());

        var point = (await ul.GetPlayerProgressionAsync("player", field: "nickname")).Single();

        await Assert.That(point.Value).IsEqualTo("$f00New name");
        await Assert.That(point.Date.Year).IsEqualTo(2026);
        await Assert.That(fake.Request.Uri.Query).IsEqualTo("?field=nickname");
    }

    [Test]
    public async Task DatesAndQueryValuesAreEscapedAndCultureIndependent()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ul = new UnitedLadder(fake.CreateClient());
        var start = new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.FromHours(2));
        var end = start.AddDays(1);

        await ul.GetPlayerProgressionAsync("player /?#", "ladderpoints", "Stadium&other=value", start, end);

        var expected = $"{UnitedLadder.BaseAddress}/progression/player%20%2F%3F%23?field=ladderpoints&envi=Stadium%26other%3Dvalue" +
            $"&startDate={start.ToUnixTimeMilliseconds()}&endDate={end.ToUnixTimeMilliseconds()}";
        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo(expected);
    }

    [Test]
    public async Task SnapshotEscapesZoneAndTrackUidAndIncludesZeroPage()
    {
        var fake = FakeHttpMessageHandler.Json("""
            [{"id":1,"type":"track","zone":"World|Czech republic","environment":"uid/?#","offset_value":0,
              "snapshot_data":{"uid":"uid/?#","name":"Map","envi":"Stadium","records":[{"id":7,"login":"player","globalRank":2,"points":45678}]},
              "created_at":"2026-11-19T12:00:00Z","expires_at":"2026-11-20T12:00:00Z"}]
            """);
        using var ul = new UnitedLadder(fake.CreateClient());

        var snapshot = await ul.GetLeaderboardSnapshotAsync("track", "World|Czech republic", "uid/?#", 0);

        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo($"{UnitedLadder.BaseAddress}/snapshot/track/World%7CCzech%20republic?envi=uid%2F%3F%23&page=0");
        await Assert.That(snapshot.Single().GetRankings().Single().GlobalRank).IsEqualTo(2);
        await Assert.That(snapshot[0].Type).IsEqualTo("track");
        await Assert.That(snapshot[0].GetTrackLeaderboard()!.Records.Single().Points).IsEqualTo(45678);
        await Assert.That(snapshot[0].ExpiresAt).IsGreaterThan(snapshot[0].CreatedAt);
    }

    [Test]
    public async Task SoloSnapshotPreservesMetadataAndTypedRankings()
    {
        var fake = FakeHttpMessageHandler.Json($$"""
            [{"id":219404,"type":"solo","zone":"World","environment":"solo","offset_value":100,
              "snapshot_data":{{RankingJson}},"created_at":"2026-11-19T12:00:00Z","expires_at":"2026-11-20T12:00:00Z"}]
            """);
        using var ul = new UnitedLadder(fake.CreateClient());

        var snapshot = (await ul.GetLeaderboardSnapshotAsync("solo")).Single();

        await Assert.That(snapshot.Id).IsEqualTo(219404);
        await Assert.That(snapshot.Offset).IsEqualTo(100);
        await Assert.That(snapshot.GetRankings().Single().Points).IsEqualTo(123456.75);
        await Assert.That(snapshot.GetTrackLeaderboard()).IsNull();
    }

    [Test]
    public async Task ReplayReturnsOriginalBinaryContent()
    {
        byte[] bytes = [0, 255, 128, 71, 66, 88];
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
        using var ul = new UnitedLadder(fake.CreateClient());

        using var response = await ul.GetReplayResponseAsync("uid/?#", 7);

        await Assert.That(await response.Content.ReadAsByteArrayAsync()).IsEquivalentTo(bytes);
        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo($"{UnitedLadder.BaseAddress}/replays/uid%2F%3F%23/7");
    }

    [Test]
    [Arguments("solo")]
    [Arguments("track")]
    [Arguments("history")]
    [Arguments("replay")]
    public async Task ErrorsKeepHttpStatus(string operation)
    {
        var fake = FakeHttpMessageHandler.Text("Upstream error", statusCode: HttpStatusCode.BadGateway);
        using var ul = new UnitedLadder(fake.CreateClient());

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => InvokeAsync(ul, operation));

        await Assert.That(error!.StatusCode).IsEqualTo(HttpStatusCode.BadGateway);
    }

    [Test]
    public async Task NullJsonBodyThrows()
    {
        var fake = FakeHttpMessageHandler.Json("null");
        using var ul = new UnitedLadder(fake.CreateClient());

        await Assert.ThrowsAsync<JsonException>(() => ul.GetSoloRankingsAsync());
    }

    [Test]
    public async Task InvalidPagingAndDatesFailBeforeSending()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ul = new UnitedLadder(fake.CreateClient());
        var end = DateTimeOffset.UtcNow;
        var start = end.AddDays(1);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ul.GetSoloRankingsAsync(page: -1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ul.GetMultiplayerRankingsAsync(page: -1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ul.GetTrackLeaderboardAsync("uid", page: -1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ul.GetPlayerMapsAsync("player", page: -1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ul.GetLeaderboardSnapshotAsync("solo", page: -1));
        await Assert.ThrowsAsync<ArgumentException>(() => ul.GetPlayerHistoryAsync("player", start, end));
        await Assert.ThrowsAsync<ArgumentException>(() => ul.GetPlayerProgressionAsync("player", startDate: start, endDate: end));
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    [Arguments("solo")]
    [Arguments("replay")]
    public async Task CancellationStopsRequests(string operation)
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ul = new UnitedLadder(fake.CreateClient());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => InvokeAsync(ul, operation, cancellation.Token));
        await Assert.That(fake.Requests).IsEmpty();
    }

    private static bool IsArray(string operation) => operation is "solo" or "multi" or "maps" or "history" or "progression" or "snapshot";

    private static async Task InvokeAsync(UnitedLadder ul, string operation, CancellationToken token = default)
    {
        switch (operation)
        {
            case "solo": await ul.GetSoloRankingsAsync(cancellationToken: token); break;
            case "multi": await ul.GetMultiplayerRankingsAsync("Stadium", page: 2, cancellationToken: token); break;
            case "track": await ul.GetTrackLeaderboardAsync("map_uid", page: 3, cancellationToken: token); break;
            case "maps": await ul.GetPlayerMapsAsync("player", 4, token); break;
            case "breakdown": await ul.GetPlayerSoloBreakdownAsync("player", token); break;
            case "login": await ul.CheckLoginAsync("player", token); break;
            case "history": await ul.GetPlayerHistoryAsync("player", cancellationToken: token); break;
            case "progression": await ul.GetPlayerProgressionAsync("player", cancellationToken: token); break;
            case "snapshot": await ul.GetLeaderboardSnapshotAsync("solo", cancellationToken: token); break;
            case "replay": using (await ul.GetReplayResponseAsync("map_uid", 7, token)) { } break;
            case "player": await ul.GetPlayerAsync("player", true, token); break;
            default: throw new ArgumentException("Unknown operation.", nameof(operation));
        }
    }
}
