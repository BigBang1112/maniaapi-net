namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class ManiaPlanetChannelFeedbackAPITests
{
    // As returned by the live API
    private const string StatusJson = """
        {"feedbackYesNo":{"maps":3791,"votesYes":977157,"votesNo":310926,"votes":1288083},
         "feedbackStars":{"maps":3553,"players":55007,"notes1Star":70489,"notes2Star":42193,"notes3Star":101709,
         "notes4Star":161511,"notes5Star":344660,"notes":720562,"uniqueNotes":413158}}
        """;

    [Test]
    public async Task GetStatusAsync_RequestsStatusRoute()
    {
        var fake = FakeHttpMessageHandler.Json(StatusJson);
        var api = CreateApi(fake);

        await api.GetStatusAsync();

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Get);
        await Assert.That(fake.Request.Url).IsEqualTo("https://feedback.prod.live.maniaplanet.com/status");
    }

    [Test]
    public async Task GetStatusAsync_ReturnsVoteAndRatingTotals()
    {
        var fake = FakeHttpMessageHandler.Json(StatusJson);
        var api = CreateApi(fake);

        var status = await api.GetStatusAsync();

        await Assert.That(status.YesNo).IsEqualTo(new ChannelYesNoFeedbackStatus(3791, 977157, 310926, 1288083));
        await Assert.That(status.Stars).IsEqualTo(new ChannelStarsFeedbackStatus(3553, 55007, 70489, 42193, 101709, 161511, 344660, 720562, 413158));
    }

    [Test]
    public async Task GetStatusAsync_Error_Throws()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.InternalServerError));
        var api = CreateApi(fake);

        await Assert.ThrowsAsync<HttpRequestException>(() => api.GetStatusAsync());
    }

    private static ManiaPlanetChannelFeedbackAPI CreateApi(FakeHttpMessageHandler fake) => new(fake.CreateClient(ManiaPlanetChannelFeedbackAPI.BaseAddress));
}
