using ManiaAPI.MasterServer.TMUF;

namespace ManiaAPI.MasterServer.Tests;

public class MasterServerTMUFTests
{
    private const int LeagueId = 5;

    [Test]
    public async Task GetScoresUrl_CombinesNumberNameAndLeague()
    {
        var url = MasterServerTMUF.GetScoresUrl(ScoresNumber.Scores3, "UnitedRace", LeagueId);

        await Assert.That(url).IsEqualTo($"http://scores.trackmaniaforever.com/scores3/UnitedRace/UnitedRace{LeagueId}.gz");
    }

    [Test]
    public async Task GetGeneralScoresUrl_UsesGeneralScores()
    {
        var url = MasterServerTMUF.GetGeneralScoresUrl(ScoresNumber.Scores3, LeagueId);

        await Assert.That(url).IsEqualTo($"http://scores.trackmaniaforever.com/scores3/General/General{LeagueId}.gz");
    }

    [Test]
    public async Task GetLadderScoresUrl_UsesMultiScores()
    {
        var url = MasterServerTMUF.GetLadderScoresUrl(ScoresNumber.Scores3, LeagueId);

        await Assert.That(url).IsEqualTo($"http://scores.trackmaniaforever.com/scores3/Multi/Multi{LeagueId}.gz");
    }

    [Test]
    public async Task Constructor_Default_UsesUnitedMasterServer()
    {
        using var server = new MasterServerTMUF();

        var baseAddress = server.Client.BaseAddress?.ToString();

        await Assert.That(baseAddress).IsEqualTo(MasterServerTMUF.DefaultUnitedUrl);
    }
}
