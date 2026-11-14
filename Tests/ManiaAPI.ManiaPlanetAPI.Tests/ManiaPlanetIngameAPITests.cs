namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class ManiaPlanetIngameAPITests
{
    private const string IngameUrl = "https://prod.live.maniaplanet.com/ingame";
    private const string Login = "bigbang1112";
    private const string TitleUid = "Nadeo_Envimix@bigbang1112";

    private static readonly string AuthJson = $$"""{"login":"{{Login}}"}""";

    [Test]
    public async Task AuthenticateAsync_SendsManiaPlanetAuthHeader()
    {
        var fake = FakeHttpMessageHandler.Json(AuthJson);
        var api = CreateApi(fake);

        await api.AuthenticateAsync(Login, "secret-token");

        await Assert.That(fake.Request.Url).IsEqualTo($"{IngameUrl}/auth");
        await Assert.That(fake.Request.Headers["Maniaplanet-Auth"]).IsEqualTo($"Login=\"{Login}\", Token=\"secret-token\"");
        await Assert.That(fake.Request.Headers["Accept"]).IsEqualTo("application/json");
    }

    [Test]
    public async Task AuthenticateAsync_ReturnsAuthenticatedLogin()
    {
        var fake = FakeHttpMessageHandler.Json(AuthJson);
        var api = CreateApi(fake);

        var result = await api.AuthenticateAsync(Login, "secret-token");

        await Assert.That(result.Login).IsEqualTo(Login);
    }

    [Test]
    public async Task AuthenticateAsync_Unauthorized_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        var api = CreateApi(fake);

        await Assert.ThrowsAsync<HttpRequestException>(() => api.AuthenticateAsync(Login, "wrong"));
    }

    [Test]
    public async Task DownloadTitleAsync_RequestsTitlePack()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        var api = CreateApi(fake);

        using var response = await api.DownloadTitleAsync(HttpMethod.Head, TitleUid);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Head);
        await Assert.That(fake.Request.Url).IsEqualTo($"{IngameUrl}/public/titles/download/{TitleUid}.Title.Pack.gbx");
    }

    [Test]
    public async Task GetTitleByUidAsync_RequestsPublicTitle()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NotFound));
        var api = CreateApi(fake);

        await api.GetTitleByUidAsync(TitleUid);

        await Assert.That(fake.Request.Url).IsEqualTo($"{IngameUrl}/public/titles/{TitleUid}");
    }

    [Test]
    public async Task GetTitleByUidAsync_NotFound_ReturnsNull()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NotFound));
        var api = CreateApi(fake);

        var title = await api.GetTitleByUidAsync("Missing@nobody");

        await Assert.That(title).IsNull();
    }

    [Test]
    public async Task SearchTitlesAsync_AddsFiltersAndOrder()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        var api = CreateApi(fake);

        await api.SearchTitlesAsync(["trackmania", "solo"], orderBy: "lastUpdate");

        await Assert.That(fake.Request.Url).IsEqualTo($"{IngameUrl}/public/titles?filters%5b%5d=trackmania&filters%5b%5d=solo&orderBy=lastUpdate");
    }

    private static ManiaPlanetIngameAPI CreateApi(FakeHttpMessageHandler fake) => new(fake.CreateClient(ManiaPlanetIngameAPI.BaseAddress));
}
