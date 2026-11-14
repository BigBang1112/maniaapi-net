namespace ManiaAPI.TrackmaniaIO.Tests;

// The public constructors share one rate limiter, so requests from parallel tests would interfere.
[NotInParallel(nameof(TrackmaniaIO))]
public class TrackmaniaIOTests
{
    private const string UserAgent = "ManiaAPI.TrackmaniaIO.Tests";

    private const string CampaignsJson = """
        {
          "campaigns": [
            {
              "id": 123,
              "name": "Fall 2024",
              "timestamp": 1727791200,
              "mapcount": 25,
              "tracked": true,
              "official": true,
              "seasonal": true,
              "weekly": false,
              "clubid": null,
              "clubname": null,
              "mediaurl": null,
              "creatorplayer": null,
              "latesteditorplayer": null
            }
          ],
          "page": 0,
          "pageCount": 4
        }
        """;

    [Test]
    public async Task Constructor_AddsCustomAndLibraryUserAgent()
    {
        using var http = new HttpClient();

        _ = new TrackmaniaIO(http, UserAgent);

        var userAgent = http.DefaultRequestHeaders.UserAgent.ToString();
        await Assert.That(userAgent).StartsWith(UserAgent);
        await Assert.That(userAgent).Contains("ManiaAPI.NET/");
        await Assert.That(userAgent).Contains("(TrackmaniaIO; Discord=bigbang1112)");
    }

    [Test]
    public async Task Constructor_SameClientTwice_DoesNotDuplicateUserAgent()
    {
        using var http = new HttpClient();

        _ = new TrackmaniaIO(http, UserAgent);
        var userAgent = http.DefaultRequestHeaders.UserAgent.ToString();
        _ = new TrackmaniaIO(http, UserAgent);

        await Assert.That(http.DefaultRequestHeaders.UserAgent.ToString()).IsEqualTo(userAgent);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Constructor_BlankUserAgent_Throws(string userAgent)
    {
        await Assert.That(() => new TrackmaniaIO(new HttpClient(), userAgent)).Throws<ArgumentException>();
    }

    [Test]
    public async Task GetSeasonalCampaignsAsync_RequestsPage()
    {
        var handler = FakeHttpMessageHandler.Json(CampaignsJson);
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await tmio.GetSeasonalCampaignsAsync(page: 2);

        await Assert.That(handler.Request.Url).IsEqualTo("https://trackmania.io/api/campaigns/seasonal/2");
    }

    [Test]
    public async Task GetSeasonalCampaignsAsync_ReturnsParsedCampaigns()
    {
        var handler = FakeHttpMessageHandler.Json(CampaignsJson);
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        var result = await tmio.GetSeasonalCampaignsAsync();

        await Assert.That(result.PageCount).IsEqualTo(4);
        await Assert.That(result.Campaigns).Count().IsEqualTo(1);

        var campaign = result.Campaigns[0];
        await Assert.That(campaign.Id).IsEqualTo(123);
        await Assert.That(campaign.Name).IsEqualTo("Fall 2024");
        await Assert.That(campaign.MapCount).IsEqualTo(25);
        await Assert.That(campaign.Timestamp).IsEqualTo(DateTimeOffset.FromUnixTimeSeconds(1727791200));
        await Assert.That(campaign.ClubId).IsNull();
    }

    [Test]
    public async Task GetClubCampaignsAsync_WithSearch_AddsSearchQuery()
    {
        var handler = FakeHttpMessageHandler.Json(CampaignsJson);
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await tmio.GetClubCampaignsAsync(page: 1, search: "rpg");

        await Assert.That(handler.Request.Url).IsEqualTo("https://trackmania.io/api/campaigns/club/1?search=rpg");
    }

    [Test]
    public async Task GetClubCampaignsAsync_WithoutSearch_OmitsSearchQuery()
    {
        var handler = FakeHttpMessageHandler.Json(CampaignsJson);
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await tmio.GetClubCampaignsAsync();

        await Assert.That(handler.Request.Url).IsEqualTo("https://trackmania.io/api/campaigns/club/0");
    }

    [Test]
    public async Task GetLeaderboardAsync_ByMapUid_AddsOffsetAndLength()
    {
        var handler = FakeHttpMessageHandler.Json("""{"tops":[],"playercount":0}""");
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await tmio.GetLeaderboardAsync("MapUid123", offset: 15, length: 50);

        await Assert.That(handler.Request.Url).IsEqualTo("https://trackmania.io/api/leaderboard/map/MapUid123?offset=15&length=50");
    }

    [Test]
    public async Task GetLeaderboardAsync_MapUidWithUrlCharacters_StaysInItsSegment()
    {
        var handler = FakeHttpMessageHandler.Json("""{"tops":[],"playercount":0}""");
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await tmio.GetLeaderboardAsync("../club/1?x=#", offset: 15, length: 50);

        await Assert.That(handler.Request.Uri.AbsoluteUri).IsEqualTo("https://trackmania.io/api/leaderboard/map/..%2Fclub%2F1%3Fx%3D%23?offset=15&length=50");
    }

    [Test]
    [Arguments(".")]
    [Arguments("..")]
    public async Task GetLeaderboardAsync_DotSegmentMapUid_ThrowsWithoutRequest(string mapUid)
    {
        var handler = FakeHttpMessageHandler.Json("{}");
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await Assert.ThrowsAsync<ArgumentException>(() => tmio.GetLeaderboardAsync(mapUid));

        await Assert.That(handler.Requests).IsEmpty();
    }

    [Test]
    public async Task GetLeaderboardAsync_EmptyMapUid_ThrowsWithoutRequest()
    {
        var handler = FakeHttpMessageHandler.Json("{}");
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await Assert.ThrowsAsync<ArgumentException>(() => tmio.GetLeaderboardAsync(""));

        await Assert.That(handler.Requests).IsEmpty();
    }

    [Test]
    public async Task GetSeasonalCampaignsAsync_ErrorStatus_Throws()
    {
        var handler = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Forbidden);
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        await Assert.ThrowsAsync<HttpRequestException>(() => tmio.GetSeasonalCampaignsAsync());
    }
}
