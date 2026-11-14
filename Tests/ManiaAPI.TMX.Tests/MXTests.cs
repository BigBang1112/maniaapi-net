namespace ManiaAPI.TMX.Tests;

public class MXTests
{
    private const string SiteUrl = "https://trackmania.exchange";
    private const int MapId = 120839;
    private const string TagsJson = """[{"ID":1,"Name":"Race","Color":"#ff0000"},{"ID":2,"Name":"FullSpeed","Color":""}]""";

    [Test]
    [Arguments(MxSite.Trackmania, "https://trackmania.exchange/")]
    [Arguments(MxSite.Maniaplanet, "https://tm.mania.exchange/")]
    [Arguments(MxSite.Shootmania, "https://sm.mania.exchange/")]
    public async Task Constructor_Site_SetsBaseAddressOfSite(MxSite site, string expectedBaseAddress)
    {
        using var mx = new MX(new HttpClient(), site);

        var baseAddress = mx.Client.BaseAddress?.ToString();

        await Assert.That(baseAddress).IsEqualTo(expectedBaseAddress);
    }

    [Test]
    public async Task Constructor_UnknownSite_ThrowsArgumentOutOfRange()
    {
        await Assert.That(() => new MX(new HttpClient(), (MxSite)999)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task GetMapGbxUrl_ReturnsSiteUrl()
    {
        using var mx = CreateMx();

        var url = mx.GetMapGbxUrl(MapId);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/mapgbx/{MapId}");
    }

    [Test]
    public async Task GetReplayGbxUrl_ReturnsSiteUrl()
    {
        using var mx = CreateMx();

        var url = mx.GetReplayGbxUrl(5);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/recordgbx/5");
    }

    [Test]
    public async Task GetMapThumbnailUrl_ReturnsSiteUrl()
    {
        using var mx = CreateMx();

        var url = mx.GetMapThumbnailUrl(MapId);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/mapthumb/{MapId}");
    }

    [Test]
    public async Task GetMappackThumbnailUrl_ReturnsSiteUrl()
    {
        using var mx = CreateMx();

        var url = mx.GetMappackThumbnailUrl(1729);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/mappackthumb/1729");
    }

    [Test]
    public async Task GetMapRandomUrl_ReturnsSiteUrl()
    {
        using var mx = CreateMx();

        var url = mx.GetMapRandomUrl();

        await Assert.That(url).IsEqualTo($"{SiteUrl}/maprandom");
    }

    [Test]
    [Arguments(null, "https://trackmania.exchange/mapimage/120839")]
    [Arguments(2, "https://trackmania.exchange/mapimage/120839/2")]
    public async Task GetMapImageUrl_AddsPositionOnlyWhenSet(int? position, string expectedUrl)
    {
        using var mx = CreateMx();

        var url = mx.GetMapImageUrl(MapId, position);

        await Assert.That(url).IsEqualTo(expectedUrl);
    }

    [Test]
    [Arguments(null, "https://trackmania.exchange/mapscreen/120839")]
    [Arguments(1, "https://trackmania.exchange/mapscreen/120839/1")]
    public async Task GetMapScreenUrl_AddsPositionOnlyWhenSet(int? position, string expectedUrl)
    {
        using var mx = CreateMx();

        var url = mx.GetMapScreenUrl(MapId, position);

        await Assert.That(url).IsEqualTo(expectedUrl);
    }

    [Test]
    public async Task GetMapImageResponseAsync_RequestsImageUrl()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        using var mx = CreateMx(fake);

        using var response = await mx.GetMapImageResponseAsync(MapId, 1);

        await Assert.That(fake.Request.Url).IsEqualTo($"{SiteUrl}/mapimage/{MapId}/1");
    }

    [Test]
    public async Task GetTagsAsync_RequestsTags()
    {
        var fake = FakeHttpMessageHandler.Json(TagsJson);
        using var mx = CreateMx(fake);

        await mx.GetTagsAsync();

        await Assert.That(fake.Request.Url).IsEqualTo($"{SiteUrl}/api/meta/tags");
    }

    [Test]
    public async Task GetTagsAsync_ReturnsParsedTags()
    {
        var fake = FakeHttpMessageHandler.Json(TagsJson);
        using var mx = CreateMx(fake);

        var tags = await mx.GetTagsAsync();

        await Assert.That(tags).IsEquivalentTo([new TagInfo(1, "Race", "#ff0000"), new TagInfo(2, "FullSpeed", "")]);
    }

    [Test]
    public async Task GetVehiclesAsync_ReturnsVehicleNames()
    {
        var fake = FakeHttpMessageHandler.Json("""["CarSport","CarSnow"]""");
        using var mx = CreateMx(fake);

        var vehicles = await mx.GetVehiclesAsync();

        await Assert.That(vehicles).IsEquivalentTo(["CarSport", "CarSnow"]);
    }

    [Test]
    public async Task GetRandomMapIdAsync_SendsHeadToRandomMap()
    {
        var fake = new FakeHttpMessageHandler(_ => RedirectedToMap());
        using var mx = CreateMx(fake);

        await mx.GetRandomMapIdAsync(new MX.SearchMapsParameters());

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Head);
        await Assert.That(fake.Request.Url).StartsWith($"{SiteUrl}/maprandom");
    }

    [Test]
    public async Task GetRandomMapIdAsync_ReturnsIdFromFinalUrl()
    {
        var fake = new FakeHttpMessageHandler(_ => RedirectedToMap());
        using var mx = CreateMx(fake);

        var mapId = await mx.GetRandomMapIdAsync(new MX.SearchMapsParameters());

        await Assert.That(mapId).IsEqualTo(MapId);
    }

    private static MX CreateMx() => new(new HttpClient(), MxSite.Trackmania);

    private static MX CreateMx(FakeHttpMessageHandler fake) => new(fake.CreateClient(), MxSite.Trackmania);

    /// <summary>
    /// A response whose final URL is a map page, the way HttpClient reports it after following the random map redirect.
    /// </summary>
    private static HttpResponseMessage RedirectedToMap() => new(HttpStatusCode.OK)
    {
        RequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{SiteUrl}/mapshow/{MapId}")
    };
}
