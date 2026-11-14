namespace ManiaAPI.ManiaPlanetAPI.Tests.Live;

/// <summary>
/// Live tests of the Channel Feedback API, whose status route needs no credentials.
/// </summary>
[LiveTest]
[NotInParallel("ManiaPlanetChannelFeedback")]
public class ManiaPlanetChannelFeedbackLiveTests
{
    [Test]
    public async Task Status_TotalsAddUp()
    {
        var api = new ManiaPlanetChannelFeedbackAPI();

        var status = await api.GetStatusAsync();
        var stars = status.Stars;

        await Assert.That(status.YesNo.Maps).IsGreaterThan(0);
        await Assert.That(status.YesNo.Votes).IsEqualTo(status.YesNo.VotesYes + status.YesNo.VotesNo);
        await Assert.That(stars.Maps).IsGreaterThan(0);
        await Assert.That(stars.Notes).IsEqualTo(stars.Notes1Star + stars.Notes2Star + stars.Notes3Star + stars.Notes4Star + stars.Notes5Star);
    }
}
