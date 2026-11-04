namespace ManiaAPI.TMX.Tests;

public class MXTests
{
    [Test]
    [Arguments(MxSite.Trackmania, "https://trackmania.exchange/")]
    [Arguments(MxSite.Maniaplanet, "https://tm.mania.exchange/")]
    [Arguments(MxSite.Shootmania, "https://sm.mania.exchange/")]
    public async Task Constructor_SetsBaseAddressForSite(MxSite site, string expectedBaseAddress)
    {
        using var mx = new MX(new HttpClient(), site);

        await Assert.That(mx.Client.BaseAddress?.ToString()).IsEqualTo(expectedBaseAddress);
    }

    [Test]
    public async Task UrlHelpers_UseSiteAddress()
    {
        using var mx = new MX(new HttpClient(), MxSite.Trackmania);

        await Assert.That(mx.GetMapGbxUrl(120839)).IsEqualTo("https://trackmania.exchange/mapgbx/120839");
        await Assert.That(mx.GetReplayGbxUrl(5)).IsEqualTo("https://trackmania.exchange/recordgbx/5");
        await Assert.That(mx.GetMapThumbnailUrl(120839)).IsEqualTo("https://trackmania.exchange/mapthumb/120839");
        await Assert.That(mx.GetMappackThumbnailUrl(1729)).IsEqualTo("https://trackmania.exchange/mappackthumb/1729");
        await Assert.That(mx.GetMapRandomUrl()).IsEqualTo("https://trackmania.exchange/maprandom");
    }

    [Test]
    [Arguments(null, "https://trackmania.exchange/mapimage/120839")]
    [Arguments(2, "https://trackmania.exchange/mapimage/120839/2")]
    public async Task GetMapImageUrl_AddsPositionOnlyWhenSet(int? position, string expectedUrl)
    {
        using var mx = new MX(new HttpClient(), MxSite.Trackmania);

        await Assert.That(mx.GetMapImageUrl(120839, position)).IsEqualTo(expectedUrl);
    }

    [Test]
    [Arguments(null, "https://trackmania.exchange/mapscreen/120839")]
    [Arguments(1, "https://trackmania.exchange/mapscreen/120839/1")]
    public async Task GetMapScreenUrl_AddsPositionOnlyWhenSet(int? position, string expectedUrl)
    {
        using var mx = new MX(new HttpClient(), MxSite.Trackmania);

        await Assert.That(mx.GetMapScreenUrl(120839, position)).IsEqualTo(expectedUrl);
    }

    [Test]
    public async Task GetMapImageResponseAsync_RequestsImageUrl()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        using var mx = new MX(fake.CreateClient(), MxSite.Trackmania);

        using var response = await mx.GetMapImageResponseAsync(120839, 1);

        await Assert.That(fake.Request.Url).IsEqualTo("https://trackmania.exchange/mapimage/120839/1");
    }

    [Test]
    public async Task GetTagsAsync_ParsesTags()
    {
        var fake = FakeHttpMessageHandler.Json("""[{"ID":1,"Name":"Race","Color":"#ff0000"},{"ID":2,"Name":"FullSpeed","Color":""}]""");
        using var mx = new MX(fake.CreateClient(), MxSite.Trackmania);

        var tags = await mx.GetTagsAsync();

        await Assert.That(fake.Request.Url).IsEqualTo("https://trackmania.exchange/api/meta/tags");
        await Assert.That(tags).IsEquivalentTo([new TagInfo(1, "Race", "#ff0000"), new TagInfo(2, "FullSpeed", "")]);
    }

    [Test]
    public async Task GetVehiclesAsync_ParsesStrings()
    {
        var fake = FakeHttpMessageHandler.Json("""["CarSport","CarSnow"]""");
        using var mx = new MX(fake.CreateClient(), MxSite.Trackmania);

        var vehicles = await mx.GetVehiclesAsync();

        await Assert.That(vehicles).IsEquivalentTo(["CarSport", "CarSnow"]);
    }

    [Test]
    public async Task GetRandomMapIdAsync_ReadsIdFromFinalUrl()
    {
        var fake = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://trackmania.exchange/mapshow/120839")
        });
        using var mx = new MX(fake.CreateClient(), MxSite.Trackmania);

        var mapId = await mx.GetRandomMapIdAsync(new MX.SearchMapsParameters());

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Head);
        await Assert.That(fake.Request.Url).StartsWith("https://trackmania.exchange/maprandom");
        await Assert.That(mapId).IsEqualTo(120839);
    }
}
