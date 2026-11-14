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

    [Test]
    public async Task GetMatchParticipantsAsync_LeaverAsNumberOrNull_ReadsBoth()
    {
        // The shape of a real cup of the day match: leaver is 1 for players who left, otherwise null.
        var fake = FakeHttpMessageHandler.Json("""
            [
              {"participant":"6a43df20-cd1a-4b3b-87b9-a6835a9b416d","position":0,"teamPosition":null,"rank":1,"score":10,"mvp":false,"leaver":null,"eliminated":false},
              {"participant":"faedcf21-d61a-4305-9ffe-680b2ee5d65e","position":1,"teamPosition":null,"rank":2,"score":5,"mvp":false,"leaver":1,"eliminated":false}
            ]
            """);
        using var nms = new NadeoMeetServices(fake.CreateClient());

        var participants = await nms.GetMatchParticipantsAsync("LID-MTCH-abc");

        await Assert.That(participants.Select(x => x.Leaver)).IsEquivalentTo(new int?[] { null, 1 });
    }
}
