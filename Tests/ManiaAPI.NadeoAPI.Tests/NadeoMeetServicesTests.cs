namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoMeetServicesTests
{
    private const string SummaryJson = """{"ranked3v3Id":2,"royalId":3,"superRoyalId":4}""";

    [Test]
    public async Task GetMatchmakingSummaryAsync_RequestsSummaryFromMeetServices()
    {
        var fake = FakeHttpMessageHandler.Json(SummaryJson);
        using var nms = new NadeoMeetServices(fake.CreateClient());

        await nms.GetMatchmakingSummaryAsync();

        await Assert.That(fake.Request.Url).IsEqualTo("https://meet.trackmania.nadeo.club/api/official/summary");
    }

    [Test]
    public async Task GetMatchmakingSummaryAsync_ReturnsSummary()
    {
        var fake = FakeHttpMessageHandler.Json(SummaryJson);
        using var nms = new NadeoMeetServices(fake.CreateClient());

        var summary = await nms.GetMatchmakingSummaryAsync();

        await Assert.That(summary).IsEqualTo(new MatchmakingSummary(Ranked3v3Id: 2, RoyalId: 3, SuperRoyalId: 4));
    }
}
