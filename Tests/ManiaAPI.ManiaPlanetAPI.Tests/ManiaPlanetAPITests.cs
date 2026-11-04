namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class ManiaPlanetAPITests
{
    private const string BaseUrl = "https://maniaplanet.com/webservices";

    private static HttpResponseMessage TokenResponse(DateTimeOffset expiresAt)
    {
        var accessToken = TestJwt.Create($$"""
            {
              "jti": "token-id",
              "aud": "client-id",
              "iat": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
              "nbf": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
              "exp": {{expiresAt.ToUnixTimeSeconds()}},
              "sub": "client-id",
              "scopes": ["basic", "titles"]
            }
            """);

        return FakeResponse.Json($$"""{"token_type":"Bearer","expires_in":3600,"access_token":"{{accessToken}}","refresh_token":"refresh"}""");
    }

    private static ManiaPlanetAPI CreateApi(FakeHttpMessageHandler fake) => new(fake.CreateClient(), new ManiaPlanetAPIHandler());

    [Test]
    public async Task AuthorizeAsync_PostsClientCredentialsToManiaPlanetLogin()
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(expiresAt));
        using var api = CreateApi(fake);

        await api.AuthorizeAsync("client-id", "client-secret", ["basic"]);

        await Assert.That(fake.Request.Url).IsEqualTo("https://prod.live.maniaplanet.com/login/oauth2/access_token");
        await Assert.That(fake.Request.Body).IsEqualTo("grant_type=client_credentials&client_id=client-id&client_secret=client-secret&scope=basic");
        await Assert.That(api.ExpirationTime).IsEqualTo(expiresAt);
        await Assert.That(api.Handler.Credentials!.Scopes).IsEquivalentTo(["basic"]);
    }

    [Test]
    public async Task Requests_AfterAuthorization_UseBearerToken()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddHours(1)),
            () => FakeResponse.Json("""[{"path":"World"},{"path":"World|Europe"}]"""));
        using var api = CreateApi(fake);
        await api.AuthorizeAsync("client-id", "client-secret");

        var zones = await api.GetZonesAsync();

        await Assert.That(fake.Requests[1].Url).IsEqualTo($"{BaseUrl}/zones");
        await Assert.That(fake.Requests[1].Authorization?.Scheme).IsEqualTo("Bearer");
        await Assert.That(zones).IsEquivalentTo(["World", "World|Europe"]);
    }

    [Test]
    [MethodDataSource(nameof(SearchTitlesCases))]
    public async Task SearchTitlesAsync_BuildsQuery(string[]? filters, string? orderBy, int offset, int length, string expectedUrl)
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var api = CreateApi(fake);

        await api.SearchTitlesAsync(filters, orderBy, offset, length);

        await Assert.That(fake.Request.Url).IsEqualTo(expectedUrl);
    }

    public static IEnumerable<Func<(string[]?, string?, int, int, string)>> SearchTitlesCases()
    {
        yield return () => (null, "onlinePlayers", 0, 10, $"{BaseUrl}/titles");
        yield return () => (["trackmania", "solo"], "onlinePlayers", 0, 10, $"{BaseUrl}/titles?filters%5b%5d=trackmania&filters%5b%5d=solo");
        yield return () => (null, "lastUpdate", 20, 5, $"{BaseUrl}/titles?orderBy=lastUpdate&offset=20&length=5");
        yield return () => (["shootmania"], null, 0, 25, $"{BaseUrl}/titles?filters%5b%5d=shootmania&length=25");
    }

    [Test]
    public async Task SearchOnlineServersAsync_DefaultsSendNoQuery()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var api = CreateApi(fake);

        await api.SearchOnlineServersAsync();

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/servers/online");
    }

    [Test]
    public async Task SearchOnlineServersAsync_AddsEveryNonDefaultParameter()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var api = CreateApi(fake);

        await api.SearchOnlineServersAsync(
            orderBy: "levelClass1",
            titleUids: ["TMStadium@nadeo", "TMCanyon@nadeo"],
            environments: ["Stadium"],
            scriptName: "TimeAttack.Script.txt",
            search: "lol",
            zone: "World|Europe",
            onlyPublic: true,
            onlyPrivate: true,
            onlyLobby: true,
            excludeLobby: false,
            offset: 10,
            length: 20);

        await Assert.That(fake.Request.Uri.Query).IsEqualTo(
            "?orderBy=levelClass1&titleUids[]=TMStadium@nadeo&titleUids[]=TMCanyon@nadeo&environments[]=Stadium" +
            "&scriptName=TimeAttack.Script.txt&search=lol&zone=World%7CEurope&onlyPublic=1&onlyPrivate=1&onlyLobby=1" +
            "&excludeLobby=0&offset=10&length=20");
    }

    [Test]
    public async Task GetTitleByUidAsync_ParsesTitle()
    {
        var fake = FakeHttpMessageHandler.Json("""
            {
              "uid": "Nadeo_Envimix@bigbang1112",
              "cost": 0,
              "name": "Nadeo Envimix",
              "description": "Description",
              "punchline": "Punchline",
              "card_url": "https://example.com/card.png",
              "download_url": "https://example.com/title.pack.gbx",
              "background_url": "https://example.com/background.png",
              "logo_url": "https://example.com/logo.png",
              "title_maker_uid": "TMStadium@nadeo",
              "public": true
            }
            """);
        using var api = CreateApi(fake);

        var title = await api.GetTitleByUidAsync("Nadeo_Envimix@bigbang1112");

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/titles/Nadeo_Envimix@bigbang1112");
        await Assert.That(title?.Name).IsEqualTo("Nadeo Envimix");
        await Assert.That(title?.TitleMakerUid).IsEqualTo("TMStadium@nadeo");
        await Assert.That(title?.Public).IsTrue();
    }

    [Test]
    public async Task GetTitleByUidAsync_NotFound_ReturnsNull()
    {
        var fake = FakeHttpMessageHandler.Json("""{"message":"Not found"}""", HttpStatusCode.NotFound);
        using var api = CreateApi(fake);

        var title = await api.GetTitleByUidAsync("Missing@nobody");

        await Assert.That(title).IsNull();
    }

    [Test]
    public async Task Forbidden_ThrowsWithoutReadingBody()
    {
        var fake = FakeHttpMessageHandler.Text("<html>Forbidden</html>", "text/html", HttpStatusCode.Forbidden);
        using var api = CreateApi(fake);

        var ex = await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetPlayerAsync());

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(ex.Response).IsNull();
    }

    [Test]
    public async Task ErrorResponse_ThrowsWithMessageFromBody()
    {
        var fake = FakeHttpMessageHandler.Json("""{"error":"invalid_request","message":"Missing scope","error_description":"The basic scope is required"}""", HttpStatusCode.BadRequest);
        using var api = CreateApi(fake);

        var ex = await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetPlayerAsync());

        await Assert.That(ex!.Message).IsEqualTo("Missing scope");
        await Assert.That(ex.Response?.ErrorDescription).IsEqualTo("The basic scope is required");
    }

    [Test]
    public async Task GetEmailAsync_ReturnsRawBody()
    {
        var fake = FakeHttpMessageHandler.Text("player@example.com");
        using var api = CreateApi(fake);

        var email = await api.GetEmailAsync();

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/me/email");
        await Assert.That(email).IsEqualTo("player@example.com");
    }
}
