namespace ManiaAPI.TMX.Tests;

public class TMXTests
{
    private const string SiteUrl = "https://tmnf.exchange";
    private const int TrackId = 2233;

    private static readonly string TracksJson = $$"""
        {
          "More": true,
          "Results": [
            {
              "TrackId": {{TrackId}},
              "TrackName": "A01-Race",
              "UId": "TrackUid",
              "AuthorTime": 23450,
              "GoldTarget": 25000,
              "Uploader": { "UserId": 42, "Name": "Nadeo" },
              "UploadedAt": "2008-04-16T12:00:00",
              "UpdatedAt": "2008-04-16T12:00:00",
              "Awards": 7,
              "HasThumbnail": true
            }
          ]
        }
        """;

    [Test]
    [Arguments(TmxSite.TMUF, "https://tmuf.exchange/")]
    [Arguments(TmxSite.TMNF, "https://tmnf.exchange/")]
    [Arguments(TmxSite.Nations, "https://nations.tm-exchange.com/")]
    [Arguments(TmxSite.Sunrise, "https://sunrise.tm-exchange.com/")]
    [Arguments(TmxSite.Original, "https://original.tm-exchange.com/")]
    public async Task Constructor_Site_SetsBaseAddressOfSite(TmxSite site, string expectedBaseAddress)
    {
        using var tmx = new TMX(new HttpClient(), site);

        var baseAddress = tmx.Client.BaseAddress?.ToString();

        await Assert.That(baseAddress).IsEqualTo(expectedBaseAddress);
    }

    [Test]
    public async Task Constructor_UnknownSite_ThrowsArgumentOutOfRange()
    {
        await Assert.That(() => new TMX(new HttpClient(), (TmxSite)999)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    [Arguments(TmxSite.TMUF)]
    [Arguments(TmxSite.Original)]
    public async Task Constructor_Site_SetsSiteName(TmxSite site)
    {
        using var tmx = new TMX(new HttpClient(), site);

        var siteName = tmx.SiteName;

        await Assert.That(siteName).IsEqualTo(site.ToString());
    }

    [Test]
    public async Task GetTrackGbxUrl_ReturnsSiteUrl()
    {
        using var tmx = CreateTmx();

        var url = tmx.GetTrackGbxUrl(TrackId);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/trackgbx/{TrackId}");
    }

    [Test]
    public async Task GetReplayGbxUrl_ReturnsSiteUrl()
    {
        using var tmx = CreateTmx();

        var url = tmx.GetReplayGbxUrl(99);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/recordgbx/99");
    }

    [Test]
    public async Task GetTrackThumbnailUrl_ReturnsFirstImage()
    {
        using var tmx = CreateTmx();

        var url = tmx.GetTrackThumbnailUrl(TrackId);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/trackshow/{TrackId}/image/0");
    }

    [Test]
    public async Task GetTrackImageUrl_ReturnsImageAtIndex()
    {
        using var tmx = CreateTmx();

        var url = tmx.GetTrackImageUrl(TrackId, 2);

        await Assert.That(url).IsEqualTo($"{SiteUrl}/trackshow/{TrackId}/image/2");
    }

    [Test]
    public async Task GetTrackRandomUrl_ReturnsSiteUrl()
    {
        using var tmx = CreateTmx();

        var url = tmx.GetTrackRandomUrl();

        await Assert.That(url).IsEqualTo($"{SiteUrl}/trackrandom");
    }

    [Test]
    public async Task GetTrackpackRandomUrl_ReturnsSiteUrl()
    {
        using var tmx = CreateTmx();

        var url = tmx.GetTrackpackRandomUrl();

        await Assert.That(url).IsEqualTo($"{SiteUrl}/trackpackrandom");
    }

    [Test]
    public async Task SearchTracksAsync_ReturnsParsedTracks()
    {
        var fake = FakeHttpMessageHandler.Json(TracksJson);
        using var tmx = CreateTmx(fake);

        var tracks = await tmx.SearchTracksAsync(new TMX.SearchTracksParameters());

        var track = tracks.Results.Single();
        await Assert.That(tracks.HasMoreItems).IsTrue();
        await Assert.That(track.TrackId).IsEqualTo(TrackId);
        await Assert.That(track.UId).IsEqualTo("TrackUid");
        await Assert.That(track.AuthorTime?.TotalMilliseconds).IsEqualTo(23450);
        await Assert.That(track.Uploader.Name).IsEqualTo("Nadeo");
        await Assert.That(track.SilverTarget).IsNull();
    }

    [Test]
    public async Task SearchTracksAsync_WithParameters_AddsEncodedQuery()
    {
        var fake = FakeHttpMessageHandler.Json(TracksJson);
        using var tmx = CreateTmx(fake);

        await tmx.SearchTracksAsync(new TMX.SearchTracksParameters
        {
            Count = 5,
            Id = [1, 2],
            Name = "A01 & co",
            AdditionalParameters = [new("custom", "a b")],
            Fields = new TrackItemFields { TrackId = true, TrackName = true }
        });

        await Assert.That(fake.Request.Url).IsEqualTo($"{SiteUrl}/api/tracks?count=5&id=1%2C2&name=A01+%26+co&custom=a+b&fields=TrackId%2CTrackName");
    }

    [Test]
    public async Task SearchTracksAsync_ErrorStatus_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.InternalServerError);
        using var tmx = CreateTmx(fake);

        await Assert.ThrowsAsync<HttpRequestException>(() => tmx.SearchTracksAsync(new TMX.SearchTracksParameters()));
    }

    [Test]
    public async Task GetReplaysAsync_WithoutFields_RequestsAllFields()
    {
        var fake = FakeHttpMessageHandler.Json("""{"More":false,"Results":[]}""");
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMUF);

        await tmx.GetReplaysAsync(new TMX.GetReplaysParameters { TrackId = 754327, Best = true });

        await Assert.That(fake.Request.Url).StartsWith("https://tmuf.exchange/api/replays?trackid=754327&best=1&fields=ReplayId%2CReplayTime%2C");
        await Assert.That(fake.Request.Url).EndsWith("%2CUser.UserId%2CUser.Name");
    }

    [Test]
    public async Task GetRandomTrackAsync_SendsHeadToRandomTrack()
    {
        var fake = new FakeHttpMessageHandler(RespondToRandomTrack);
        using var tmx = CreateTmx(fake);

        await tmx.GetRandomTrackAsync(new TMX.SearchTracksParameters { Name = "A01" });

        await Assert.That(fake.Requests[0].Method).IsEqualTo(HttpMethod.Head);
        await Assert.That(fake.Requests[0].Url).IsEqualTo($"{SiteUrl}/trackrandom?name=A01");
    }

    [Test]
    public async Task GetRandomTrackAsync_RequestsTrackFromFinalUrl()
    {
        var fake = new FakeHttpMessageHandler(RespondToRandomTrack);
        using var tmx = CreateTmx(fake);

        await tmx.GetRandomTrackAsync(new TMX.SearchTracksParameters());

        await Assert.That(fake.Requests[1].Url).StartsWith($"{SiteUrl}/api/tracks?count=1&id={TrackId}&fields=");
    }

    [Test]
    public async Task GetRandomTrackAsync_ReturnsTrack()
    {
        var fake = new FakeHttpMessageHandler(RespondToRandomTrack);
        using var tmx = CreateTmx(fake);

        var track = await tmx.GetRandomTrackAsync(new TMX.SearchTracksParameters());

        await Assert.That(track?.TrackId).IsEqualTo(TrackId);
    }

    [Test]
    public async Task GetRandomTrackIdAsync_NotFound_ReturnsNull()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NotFound));
        using var tmx = CreateTmx(fake);

        var trackId = await tmx.GetRandomTrackIdAsync(new TMX.SearchTracksParameters());

        await Assert.That(trackId).IsNull();
    }

    [Test]
    public async Task GetRandomTrackIdAsync_NoTrackIdInFinalUrl_ReturnsNull()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        using var tmx = CreateTmx(fake);

        var trackId = await tmx.GetRandomTrackIdAsync(new TMX.SearchTracksParameters());

        await Assert.That(trackId).IsNull();
    }

    private static TMX CreateTmx() => new(new HttpClient(), TmxSite.TMNF);

    private static TMX CreateTmx(FakeHttpMessageHandler fake) => new(fake.CreateClient(), TmxSite.TMNF);

    /// <summary>
    /// Answers the HEAD request to the random track page as if it redirected to a track, and any other request with that track.
    /// </summary>
    private static HttpResponseMessage RespondToRandomTrack(HttpRequestMessage request)
    {
        if (request.Method != HttpMethod.Head)
        {
            return FakeResponse.Json(TracksJson);
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            // HttpClient reports the final URL after following the redirect.
            RequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{SiteUrl}/trackshow/{TrackId}")
        };
    }
}
