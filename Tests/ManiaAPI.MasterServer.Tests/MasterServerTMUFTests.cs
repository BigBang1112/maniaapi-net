using ManiaAPI.MasterServer.TMUF;

namespace ManiaAPI.MasterServer.Tests;

public class MasterServerTMUFTests
{
    [Test]
    public async Task GetScoresUrl_CombinesNumberNameAndLeague()
    {
        var url = MasterServerTMUF.GetScoresUrl(ScoresNumber.Scores3, "UnitedRace", leagueId: 5);

        await Assert.That(url).IsEqualTo("http://scores.trackmaniaforever.com/scores3/UnitedRace/UnitedRace5.gz");
    }

    [Test]
    public async Task GetGeneralScoresUrl_UsesGeneralScores()
    {
        var url = MasterServerTMUF.GetGeneralScoresUrl(ScoresNumber.Scores3, leagueId: 5);

        await Assert.That(url).IsEqualTo("http://scores.trackmaniaforever.com/scores3/General/General5.gz");
    }

    [Test]
    public async Task GetLadderScoresUrl_UsesMultiScores()
    {
        var url = MasterServerTMUF.GetLadderScoresUrl(ScoresNumber.Scores3, leagueId: 5);

        await Assert.That(url).IsEqualTo("http://scores.trackmaniaforever.com/scores3/Multi/Multi5.gz");
    }

    [Test]
    public async Task DefaultConstructor_UsesUnitedMasterServer()
    {
        using var server = new MasterServerTMUF();

        await Assert.That(server.Client.BaseAddress?.ToString()).IsEqualTo(MasterServerTMUF.DefaultUnitedUrl);
    }
}
