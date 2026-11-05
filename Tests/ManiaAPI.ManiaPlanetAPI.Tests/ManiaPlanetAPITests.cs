namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class ManiaPlanetAPITests
{
    private const string BaseUrl = "https://maniaplanet.com/webservices";
    private const string ClientId = "client-id";
    private const string ClientSecret = "client-secret";
    private const string TitleUid = "Nadeo_Envimix@bigbang1112";
    private const string ZonesJson = """[{"path":"World"},{"path":"World|Europe"}]""";

    private static readonly DateTimeOffset ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);

    private static readonly string TitleJson = $$"""
        {
          "uid": "{{TitleUid}}",
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
        """;

    [Test]
    public async Task AuthorizeAsync_PostsClientCredentialsToManiaPlanetLogin()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(ExpiresAt));
        using var api = CreateApi(fake);

        await api.AuthorizeAsync(ClientId, ClientSecret, ["basic"]);

        await Assert.That(fake.Request.Url).IsEqualTo("https://prod.live.maniaplanet.com/login/oauth2/access_token");
        await Assert.That(fake.Request.Body).IsEqualTo($"grant_type=client_credentials&client_id={ClientId}&client_secret={ClientSecret}&scope=basic");
    }

    [Test]
    public async Task AuthorizeAsync_ValidToken_StoresExpirationAndScopes()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(ExpiresAt));
        using var api = CreateApi(fake);

        await api.AuthorizeAsync(ClientId, ClientSecret, ["basic"]);

        await Assert.That(api.ExpirationTime).IsEqualTo(ExpiresAt);
        await Assert.That(api.Handler.Credentials!.Scopes).IsEquivalentTo(["basic"]);
    }

    [Test]
    public async Task GetZonesAsync_AfterAuthorization_SendsBearerToken()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddHours(1)),
            () => FakeResponse.Json(ZonesJson));
        using var api = CreateApi(fake);
        await api.AuthorizeAsync(ClientId, ClientSecret);

        await api.GetZonesAsync();

        await Assert.That(fake.Requests[1].Authorization?.Scheme).IsEqualTo("Bearer");
    }

    [Test]
    public async Task GetZonesAsync_RequestsZones()
    {
        var fake = FakeHttpMessageHandler.Json(ZonesJson);
        using var api = CreateApi(fake);

        await api.GetZonesAsync();

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/zones");
    }

    [Test]
    public async Task GetZonesAsync_ReturnsZonePaths()
    {
        var fake = FakeHttpMessageHandler.Json(ZonesJson);
        using var api = CreateApi(fake);

        var zones = await api.GetZonesAsync();

        await Assert.That(zones).IsEquivalentTo(["World", "World|Europe"]);
    }

    [Test]
    [MethodDataSource(nameof(SearchTitlesCases))]
    public async Task SearchTitlesAsync_AddsOnlyNonDefaultParameters(string[]? filters, string? orderBy, int offset, int length, string expectedUrl)
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
    public async Task SearchOnlineServersAsync_Defaults_SendsNoQuery()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        using var api = CreateApi(fake);

        await api.SearchOnlineServersAsync();

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/servers/online");
    }

    [Test]
    public async Task SearchOnlineServersAsync_EveryParameterSet_AddsEveryParameter()
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
    public async Task GetTitleByUidAsync_RequestsTitle()
    {
        var fake = FakeHttpMessageHandler.Json(TitleJson);
        using var api = CreateApi(fake);

        await api.GetTitleByUidAsync(TitleUid);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/titles/{TitleUid}");
    }

    [Test]
    public async Task GetTitleByUidAsync_ReturnsParsedTitle()
    {
        var fake = FakeHttpMessageHandler.Json(TitleJson);
        using var api = CreateApi(fake);

        var title = await api.GetTitleByUidAsync(TitleUid);

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
    public async Task GetPlayerAsync_ForbiddenHtml_ThrowsWithoutResponse()
    {
        var fake = FakeHttpMessageHandler.Text("<html>Forbidden</html>", "text/html", HttpStatusCode.Forbidden);
        using var api = CreateApi(fake);

        var ex = await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetPlayerAsync());

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(ex.Response).IsNull();
    }

    [Test]
    public async Task GetPlayerAsync_ErrorResponse_ThrowsWithMessageFromBody()
    {
        var fake = FakeHttpMessageHandler.Json("""{"error":"invalid_request","message":"Missing scope","error_description":"The basic scope is required"}""", HttpStatusCode.BadRequest);
        using var api = CreateApi(fake);

        var ex = await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetPlayerAsync());

        await Assert.That(ex!.Message).IsEqualTo("Missing scope");
        await Assert.That(ex.Response?.ErrorDescription).IsEqualTo("The basic scope is required");
    }

    [Test]
    public async Task GetEmailAsync_RequestsEmail()
    {
        var fake = FakeHttpMessageHandler.Text("player@example.com");
        using var api = CreateApi(fake);

        await api.GetEmailAsync();

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/me/email");
    }

    [Test]
    public async Task GetEmailAsync_ReturnsRawBody()
    {
        var fake = FakeHttpMessageHandler.Text("player@example.com");
        using var api = CreateApi(fake);

        var email = await api.GetEmailAsync();

        await Assert.That(email).IsEqualTo("player@example.com");
    }

    private static ManiaPlanetAPI CreateApi(FakeHttpMessageHandler fake) => new(fake.CreateClient(), new ManiaPlanetAPIHandler());

    private static HttpResponseMessage TokenResponse(DateTimeOffset expiresAt)
    {
        var accessToken = TestJwt.Create($$"""
            {
              "jti": "token-id",
              "aud": "{{ClientId}}",
              "iat": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
              "nbf": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
              "exp": {{expiresAt.ToUnixTimeSeconds()}},
              "sub": "{{ClientId}}",
              "scopes": ["basic", "titles"]
            }
            """);

        return FakeResponse.Json($$"""{"token_type":"Bearer","expires_in":3600,"access_token":"{{accessToken}}","refresh_token":"refresh"}""");
    }
}
