using System.Text;

namespace ManiaAPI.TrackmaniaWS.Tests;

public class TrackmaniaWSTests
{
    private const string ApiUser = "api-user";
    private const string ApiPassword = "api-password";
    private const string Login = "bigbang1112";

    private static readonly string ExpectedBasicCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ApiUser}:{ApiPassword}"));

    // As returned by the live test route
    private const string FoobarJson = """
        ["GET",{"anInt":1337,"aString":"quarante-deux","anObject":{"someBlabla":["Hello world",1337],"foo":"bar"},"anArray":["un","deux","trois",4]}]
        """;

    [Test]
    public async Task PingAsync_RequestsTestRouteWithoutCredentials()
    {
        var fake = FakeHttpMessageHandler.Json(FoobarJson);
        using var ws = CreateWs(fake);

        var up = await ws.PingAsync();

        await Assert.That(up).IsTrue();
        await Assert.That(fake.Request.Url).IsEqualTo("http://ws.trackmania.com/foobar/index.json");
        await Assert.That(fake.Request.Authorization).IsNull();
    }

    [Test]
    [Arguments("""["GET",{"anInt":42}]""")]
    [Arguments("""{"anInt":1337}""")]
    [Arguments("""["POST",{"anInt":1337}]""")]
    [Arguments("<html>Maintenance</html>")]
    public async Task PingAsync_UnexpectedResponse_ReturnsFalse(string body)
    {
        var fake = FakeHttpMessageHandler.Text(body, "application/json");
        using var ws = CreateWs(fake);

        await Assert.That(await ws.PingAsync()).IsFalse();
    }

    [Test]
    public async Task PingAsync_ServerError_ReturnsFalse()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.ServiceUnavailable));
        using var ws = CreateWs(fake);

        await Assert.That(await ws.PingAsync()).IsFalse();
    }

    [Test]
    public async Task GetPlayerAsync_RequestsPlayer()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = CreateWs(fake);

        await ws.GetPlayerAsync(Login);

        await Assert.That(fake.Request.Url).IsEqualTo($"http://ws.trackmania.com/tmf/players/{Login}/");
    }

    [Test]
    public async Task GetPlayerAsync_SendsBasicAuth()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = CreateWs(fake);

        await ws.GetPlayerAsync(Login);

        await Assert.That(fake.Request.Authorization?.Scheme).IsEqualTo("Basic");
        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(ExpectedBasicCredentials);
    }

    [Test]
    public async Task GetPlayerAsync_WithCredentialsRecord_SendsSameBasicAuth()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = new TrackmaniaWS(new TrackmaniaWSCredentials(ApiUser, ApiPassword), fake.CreateClient(TrackmaniaWS.BaseAddress));

        await ws.GetPlayerAsync(Login);

        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(ExpectedBasicCredentials);
    }

    [Test]
    public async Task GetPlayerAsync_StringValues_ReturnsParsedPlayer()
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson());
        using var ws = CreateWs(fake);

        var player = await ws.GetPlayerAsync(Login);

        await Assert.That(player).IsEqualTo(new Player(123, Login, "$f00Big", United: true, "World|Europe|Czech republic", IdZone: 5));
    }

    [Test]
    [Arguments("0", false)]
    [Arguments("", false)]
    [Arguments("1", true)]
    public async Task GetPlayerAsync_UnitedIsTrueOnlyForOne(string united, bool expected)
    {
        var fake = FakeHttpMessageHandler.Json(PlayerJson(united));
        using var ws = CreateWs(fake);

        var player = await ws.GetPlayerAsync(Login);

        await Assert.That(player.United).IsEqualTo(expected);
    }

    [Test]
    public async Task GetPlayerAsync_Unauthorized_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        using var ws = CreateWs(fake);

        await Assert.ThrowsAsync<HttpRequestException>(() => ws.GetPlayerAsync(Login));
    }

    [Test]
    public async Task GetZoneByPathAsync_EscapesPath()
    {
        var fake = FakeHttpMessageHandler.Json("""{"id":"310","name":"Czech republic","path":"World|Czech republic"}""");
        using var ws = CreateWs(fake);

        await ws.GetZoneByPathAsync("World|Czech republic");

        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo("http://ws.trackmania.com/tmf/zones/path/World%7CCzech%20republic/");
    }

    [Test]
    [Arguments(null, false, "sort=&order=")]
    [Arguments(ZoneSort.Id, false, "sort=id&order=1")]
    [Arguments(ZoneSort.Name, true, "sort=name&order=-1")]
    public async Task GetZonesAsync_SendsPagingAndSorting(ZoneSort? sort, bool descending, string expectedSorting)
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var ws = CreateWs(fake);

        await ws.GetZonesAsync(offset: 20, length: 5, sort, descending);

        await Assert.That(fake.Request.Url).IsEqualTo($"http://ws.trackmania.com/tmf/zones/all/?offset=20&length=5&{expectedSorting}");
    }

    [Test]
    public async Task GetZonesAsync_ParsesPathIds()
    {
        var fake = FakeHttpMessageHandler.Json("""[{"id":"1","name":"World","path":"World","pathIds":""},{"id":"109328","name":"X","path":"World|China|X","pathIds":"1,330"}]""");
        using var ws = CreateWs(fake);

        var zones = await ws.GetZonesAsync();

        await Assert.That(zones[0].PathIds).IsEmpty();
        await Assert.That(zones[1].PathIds).IsEquivalentTo([1, 330]);
    }

    [Test]
    public async Task GetPlayerSoloRankingAsync_ReadsNumbersAndStrings()
    {
        // The solo ranking sends idZone as a number, unlike the other routes
        var fake = FakeHttpMessageHandler.Json("""{"environment":"Merge","unit":"Skill Points","points":"3515275","ranks":[{"idZone":1,"path":"World","rank":"224"}]}""");
        using var ws = CreateWs(fake);

        var ranking = await ws.GetPlayerSoloRankingAsync(Login);

        await Assert.That(ranking.Points).IsEqualTo(3515275m);
        await Assert.That(ranking.Ranks.Single()).IsEqualTo(new PlayerZoneRank(1, "World", 224));
    }

    [Test]
    public async Task GetPlayerMultiplayerRankingAsync_ParsesDecimalPoints()
    {
        var fake = FakeHttpMessageHandler.Json("""{"environment":"Stadium","unit":"Ladder Points","points":"63960.27","ranks":[]}""");
        using var ws = CreateWs(fake);

        var ranking = await ws.GetPlayerMultiplayerRankingAsync(Login, "Stadium");

        await Assert.That(fake.Request.Url).IsEqualTo($"http://ws.trackmania.com/tmf/players/{Login}/rankings/multiplayer/Stadium/");
        await Assert.That(ranking.Points).IsEqualTo(63960.27m);
    }

    [Test]
    [Arguments(null, "http://ws.trackmania.com/tmf/rankings/solo/challenges/uid/players/?offset=0&length=10")]
    [Arguments("World|France", "http://ws.trackmania.com/tmf/rankings/solo/challenges/uid/players/World%7CFrance/?offset=0&length=10")]
    public async Task GetSoloChallengeLeaderboardAsync_RequestsZoneWhenGiven(string? path, string expectedUrl)
    {
        var fake = FakeHttpMessageHandler.Json("""{"idZone":"1","path":"World","challengeUID":"uid","gameMode":"Race","unit":"Score","players":[]}""");
        using var ws = CreateWs(fake);

        var leaderboard = await ws.GetSoloChallengeLeaderboardAsync("uid", path);

        await Assert.That(fake.Request.Uri.AbsoluteUri).IsEqualTo(expectedUrl);
        await Assert.That(leaderboard.ChallengeUid).IsEqualTo("uid");
    }

    [Test]
    public async Task GetZoneIdAsync_ParsesNumberInString()
    {
        var fake = FakeHttpMessageHandler.Json("\"310\"");
        using var ws = CreateWs(fake);

        await Assert.That(await ws.GetZoneIdAsync("World|Czech republic")).IsEqualTo(310);
    }

    [Test]
    public async Task GetRegisteredPlayerCountAsync_ParsesNumberInString()
    {
        var fake = FakeHttpMessageHandler.Json("\"18468327\"");
        using var ws = CreateWs(fake);

        await Assert.That(await ws.GetRegisteredPlayerCountAsync()).IsEqualTo(18468327);
        await Assert.That(fake.Request.Url).IsEqualTo("http://ws.trackmania.com/tmf/registrations/");
    }

    [Test]
    public async Task GetPlayerAsync_ErrorResponse_ThrowsWithServerMessage()
    {
        var fake = FakeHttpMessageHandler.Json("""{"message":"Unkown player"}""", HttpStatusCode.NotFound);
        using var ws = CreateWs(fake);

        var ex = await Assert.ThrowsAsync<TrackmaniaWSResponseException>(() => ws.GetPlayerAsync("nobody"));

        await Assert.That(ex!.Message).IsEqualTo("Unkown player");
        await Assert.That(ex.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetPlayerAsync_ErrorWithInvalidCharset_ThrowsResponseException()
    {
        var fake = new FakeHttpMessageHandler(_ =>
        {
            var content = new StringContent("Service unavailable");
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/plain; charset=bogus");
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = content };
        });
        using var ws = CreateWs(fake);

        var ex = await Assert.ThrowsAsync<TrackmaniaWSResponseException>(() => ws.GetPlayerAsync("nobody"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.ServiceUnavailable);
    }

    [Test]
    public async Task GetZonesAsync_EmptyErrorMessage_FallsBackToStatus()
    {
        var fake = FakeHttpMessageHandler.Json("""{"message":""}""", HttpStatusCode.BadRequest);
        using var ws = CreateWs(fake);

        var ex = await Assert.ThrowsAsync<TrackmaniaWSResponseException>(() => ws.GetZonesAsync(length: 101));

        await Assert.That(ex!.Message).Contains("400");
    }

    [Test]
    public async Task SendManiaHomePrivateNotificationAsync_PostsNotification()
    {
        var fake = FakeHttpMessageHandler.Json("null");
        using var ws = CreateWs(fake);

        await ws.SendManiaHomePrivateNotificationAsync("my_manialink", Login, "Hello", "my_manialink?page=2");

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"http://ws.trackmania.com/maniahome/notification/{Login}/private/");
        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(ExpectedBasicCredentials);
        await Assert.That(fake.Request.Body).IsEqualTo($$"""{"senderName":"my_manialink","receiverName":"{{Login}}","message":"Hello","link":"my_manialink?page=2","iconStyle":null,"iconSubStyle":null}""");
    }

    [Test]
    public async Task SendManiaHomeNotificationAsync_PostsWithoutReceiver()
    {
        var fake = FakeHttpMessageHandler.Json("null");
        using var ws = CreateWs(fake);

        await ws.SendManiaHomeNotificationAsync("my_manialink", "Hello", iconStyle: "Icons128x128_1", iconSubStyle: "Vehicles");

        await Assert.That(fake.Request.Url).IsEqualTo("http://ws.trackmania.com/maniahome/notification/");
        await Assert.That(fake.Request.Body).Contains("\"receiverName\":null");
        await Assert.That(fake.Request.Body).Contains("\"iconSubStyle\":\"Vehicles\"");
    }

    private static TrackmaniaWS CreateWs(FakeHttpMessageHandler fake) => new(ApiUser, ApiPassword, fake.CreateClient(TrackmaniaWS.BaseAddress));

    /// <summary>
    /// A player as the web services return it, with every value as a string.
    /// </summary>
    private static string PlayerJson(string united = "1") => $$"""
        {
          "id": "123",
          "login": "{{Login}}",
          "nickname": "$f00Big",
          "united": "{{united}}",
          "path": "World|Europe|Czech republic",
          "idZone": "5"
        }
        """;
}
