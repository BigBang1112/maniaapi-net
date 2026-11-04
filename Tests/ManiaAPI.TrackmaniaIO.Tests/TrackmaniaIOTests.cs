namespace ManiaAPI.TrackmaniaIO.Tests;

// TrackmaniaIO keeps its rate limit state in static fields, so requests from parallel tests would interfere.
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
    public async Task GetSeasonalCampaignsAsync_RequestsPageAndParsesCampaigns()
    {
        var handler = FakeHttpMessageHandler.Json(CampaignsJson);
        using var tmio = new TrackmaniaIO(handler.CreateClient(), UserAgent);

        var result = await tmio.GetSeasonalCampaignsAsync(page: 2);

        await Assert.That(handler.Request.Url).IsEqualTo("https://trackmania.io/api/campaigns/seasonal/2");
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

    [Test]
    public async Task GetHeaderNumberValue_NumericHeader_ReturnsValue()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Add("X-Ratelimit-Remaining", "123");

        var value = TrackmaniaIO.GetHeaderNumberValue("X-Ratelimit-Remaining", response);

        await Assert.That(value).IsEqualTo(123);
    }

    [Test]
    public async Task GetHeaderNumberValue_MissingHeader_ReturnsNull()
    {
        using var response = new HttpResponseMessage();

        var value = TrackmaniaIO.GetHeaderNumberValue("X-Ratelimit-Remaining", response);

        await Assert.That(value).IsNull();
    }

    [Test]
    public async Task GetHeaderNumberValue_NonNumericHeader_ReturnsNull()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Add("X-Ratelimit-Remaining", "value");

        var value = TrackmaniaIO.GetHeaderNumberValue("X-Ratelimit-Remaining", response);

        await Assert.That(value).IsNull();
    }
}
