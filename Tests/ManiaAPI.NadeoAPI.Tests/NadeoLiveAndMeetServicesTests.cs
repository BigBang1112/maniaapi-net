namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoLiveAndMeetServicesTests
{
    [Test]
    public async Task NadeoLiveServices_GetTopLeaderboardAsync_UsesLiveServicesAddress()
    {
        var fake = FakeHttpMessageHandler.Json("""{"groupUid":"Personal_Best","mapUid":"UidOfTheMap","tops":[]}""");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        var leaderboard = await nls.GetTopLeaderboardAsync("UidOfTheMap", length: 5, offset: 10);

        await Assert.That(fake.Request.Url).StartsWith(
            "https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/Personal_Best/map/UidOfTheMap/top?length=5&offset=10&onlyWorld=");
        await Assert.That(leaderboard.MapUid).IsEqualTo("UidOfTheMap");
        await Assert.That(leaderboard.Tops).IsEmpty();
    }

    [Test]
    public async Task NadeoLiveServices_GetTopLeaderboardAsync_EmptyUid_ThrowsWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await Assert.ThrowsAsync<ArgumentException>(() => nls.GetTopLeaderboardAsync(""));
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task NadeoMeetServices_GetMatchmakingSummaryAsync_UsesMeetAddress()
    {
        var fake = FakeHttpMessageHandler.Json("""{"ranked3v3Id":2,"royalId":3,"superRoyalId":4}""");
        using var nms = new NadeoMeetServices(fake.CreateClient());

        var summary = await nms.GetMatchmakingSummaryAsync();

        await Assert.That(fake.Request.Url).IsEqualTo("https://meet.trackmania.nadeo.club/api/official/summary");
        await Assert.That(summary).IsEqualTo(new MatchmakingSummary(Ranked3v3Id: 2, RoyalId: 3, SuperRoyalId: 4));
    }
}
