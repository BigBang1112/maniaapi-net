namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class ManiaPlanetIngameAPITests
{
    private static ManiaPlanetIngameAPI CreateApi(FakeHttpMessageHandler fake) => new(fake.CreateClient(ManiaPlanetIngameAPI.BaseAddress));

    [Test]
    public async Task AuthenticateAsync_SendsManiaPlanetAuthHeader()
    {
        var fake = FakeHttpMessageHandler.Json("""{"login":"bigbang1112"}""");
        var api = CreateApi(fake);

        var result = await api.AuthenticateAsync("bigbang1112", "secret-token");

        await Assert.That(fake.Request.Url).IsEqualTo("https://prod.live.maniaplanet.com/ingame/auth");
        await Assert.That(fake.Request.Headers["Maniaplanet-Auth"]).IsEqualTo("Login=\"bigbang1112\", Token=\"secret-token\"");
        await Assert.That(fake.Request.Headers["Accept"]).IsEqualTo("application/json");
        await Assert.That(result.Login).IsEqualTo("bigbang1112");
    }

    [Test]
    public async Task AuthenticateAsync_Unauthorized_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        var api = CreateApi(fake);

        await Assert.ThrowsAsync<HttpRequestException>(() => api.AuthenticateAsync("bigbang1112", "wrong"));
    }

    [Test]
    public async Task DownloadTitleAsync_UsesTitlePackPath()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        var api = CreateApi(fake);

        using var response = await api.DownloadTitleAsync(HttpMethod.Head, "Nadeo_Envimix@bigbang1112");

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Head);
        await Assert.That(fake.Request.Url).IsEqualTo("https://prod.live.maniaplanet.com/ingame/public/titles/download/Nadeo_Envimix@bigbang1112.Title.Pack.gbx");
    }

    [Test]
    public async Task GetTitleByUidAsync_NotFound_ReturnsNull()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NotFound));
        var api = CreateApi(fake);

        var title = await api.GetTitleByUidAsync("Missing@nobody");

        await Assert.That(fake.Request.Url).IsEqualTo("https://prod.live.maniaplanet.com/ingame/public/titles/Missing@nobody");
        await Assert.That(title).IsNull();
    }

    [Test]
    public async Task SearchTitlesAsync_BuildsQuery()
    {
        var fake = FakeHttpMessageHandler.Json("[]");
        var api = CreateApi(fake);

        await api.SearchTitlesAsync(["trackmania", "solo"], orderBy: "lastUpdate");

        await Assert.That(fake.Request.Url).IsEqualTo("https://prod.live.maniaplanet.com/ingame/public/titles?filters%5b%5d=trackmania&filters%5b%5d=solo&orderBy=lastUpdate");
    }
}
