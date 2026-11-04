namespace ManiaAPI.TMX.Tests;

public class TMXTests
{
    private const string TracksJson = """
        {
          "More": true,
          "Results": [
            {
              "TrackId": 2233,
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
    public async Task Constructor_SetsBaseAddressForSite(TmxSite site, string expectedBaseAddress)
    {
        using var tmx = new TMX(new HttpClient(), site);

        await Assert.That(tmx.Client.BaseAddress?.ToString()).IsEqualTo(expectedBaseAddress);
        await Assert.That(tmx.SiteName).IsEqualTo(site.ToString());
    }

    [Test]
    public async Task UrlHelpers_UseSiteAddress()
    {
        using var tmx = new TMX(new HttpClient(), TmxSite.TMNF);

        await Assert.That(tmx.GetTrackGbxUrl(2233)).IsEqualTo("https://tmnf.exchange/trackgbx/2233");
        await Assert.That(tmx.GetReplayGbxUrl(99)).IsEqualTo("https://tmnf.exchange/recordgbx/99");
        await Assert.That(tmx.GetTrackThumbnailUrl(2233)).IsEqualTo("https://tmnf.exchange/trackshow/2233/image/0");
        await Assert.That(tmx.GetTrackImageUrl(2233, 2)).IsEqualTo("https://tmnf.exchange/trackshow/2233/image/2");
        await Assert.That(tmx.GetTrackRandomUrl()).IsEqualTo("https://tmnf.exchange/trackrandom");
        await Assert.That(tmx.GetTrackpackRandomUrl()).IsEqualTo("https://tmnf.exchange/trackpackrandom");
    }

    [Test]
    public async Task SearchTracksAsync_ParsesResults()
    {
        var fake = FakeHttpMessageHandler.Json(TracksJson);
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMNF);

        var tracks = await tmx.SearchTracksAsync(new TMX.SearchTracksParameters());

        await Assert.That(tracks.HasMoreItems).IsTrue();
        var track = tracks.Results.Single();
        await Assert.That(track.TrackId).IsEqualTo(2233);
        await Assert.That(track.UId).IsEqualTo("TrackUid");
        await Assert.That(track.AuthorTime?.TotalMilliseconds).IsEqualTo(23450);
        await Assert.That(track.Uploader.Name).IsEqualTo("Nadeo");
        await Assert.That(track.SilverTarget).IsNull();
    }

    [Test]
    public async Task SearchTracksAsync_BuildsQueryWithEncodedValues()
    {
        var fake = FakeHttpMessageHandler.Json(TracksJson);
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMNF);

        await tmx.SearchTracksAsync(new TMX.SearchTracksParameters
        {
            Count = 5,
            Id = [1, 2],
            Name = "A01 & co",
            AdditionalParameters = [new("custom", "a b")],
            Fields = new TrackItemFields { TrackId = true, TrackName = true }
        });

        await Assert.That(fake.Request.Url).IsEqualTo("https://tmnf.exchange/api/tracks?count=5&id=1%2C2&name=A01+%26+co&custom=a+b&fields=TrackId%2CTrackName");
    }

    [Test]
    public async Task GetReplaysAsync_DefaultParametersRequestAllFields()
    {
        var fake = FakeHttpMessageHandler.Json("""{"More":false,"Results":[]}""");
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMUF);

        var replays = await tmx.GetReplaysAsync(new TMX.GetReplaysParameters { TrackId = 754327, Best = true });

        await Assert.That(fake.Request.Url).StartsWith("https://tmuf.exchange/api/replays?trackid=754327&best=1&fields=ReplayId%2CReplayTime%2C");
        await Assert.That(fake.Request.Url).EndsWith("%2CUser.UserId%2CUser.Name");
        await Assert.That(replays.Results).IsEmpty();
    }

    [Test]
    public async Task SearchTracksAsync_ErrorStatus_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.InternalServerError);
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMNF);

        await Assert.ThrowsAsync<HttpRequestException>(() => tmx.SearchTracksAsync(new TMX.SearchTracksParameters()));
    }

    [Test]
    public async Task GetRandomTrackAsync_FollowsRedirectAndLoadsTrack()
    {
        var fake = new FakeHttpMessageHandler(request => request.Method == HttpMethod.Head
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                // HttpClient reports the final URL after following the redirect.
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://tmnf.exchange/trackshow/2233")
            }
            : FakeResponse.Json(TracksJson));
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMNF);

        var track = await tmx.GetRandomTrackAsync(new TMX.SearchTracksParameters { Name = "A01" });

        await Assert.That(fake.Requests[0].Method).IsEqualTo(HttpMethod.Head);
        await Assert.That(fake.Requests[0].Url).IsEqualTo("https://tmnf.exchange/trackrandom?name=A01");
        await Assert.That(fake.Requests[1].Url).StartsWith("https://tmnf.exchange/api/tracks?count=1&id=2233&fields=");
        await Assert.That(track?.TrackId).IsEqualTo(2233);
    }

    [Test]
    public async Task GetRandomTrackIdAsync_NotFound_ReturnsNull()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NotFound));
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMNF);

        var trackId = await tmx.GetRandomTrackIdAsync(new TMX.SearchTracksParameters());

        await Assert.That(trackId).IsNull();
    }

    [Test]
    public async Task GetRandomTrackIdAsync_NoTrackIdInFinalUrl_ReturnsNull()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.OK));
        using var tmx = new TMX(fake.CreateClient(), TmxSite.TMNF);

        var trackId = await tmx.GetRandomTrackIdAsync(new TMX.SearchTracksParameters());

        await Assert.That(trackId).IsNull();
    }
}
