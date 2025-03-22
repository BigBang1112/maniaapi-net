using ManiaAPI.XmlRpc.TMUF;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.XmlRpc.Tests.Unit;

public class MasterServerTMUFTests
{
    [Fact]
    public void GetScoresUrl_ReturnsCorrectUrl()
    {
        var expectedUrl = "http://scores.trackmaniaforever.com/scores3/UnitedRace/UnitedRace5.gz";

        var actualUrl = MasterServerTMUF.GetScoresUrl(ScoresNumber.Scores3, "UnitedRace", zoneId: 5);

        Assert.Equal(expectedUrl, actualUrl);
    }
    
    [Fact]
    public void GetGeneralScoresUrl_ReturnsCorrectUrl()
    {
        var expectedUrl = "http://scores.trackmaniaforever.com/scores3/General/General5.gz";

        var actualUrl = MasterServerTMUF.GetGeneralScoresUrl(ScoresNumber.Scores3, zoneId: 5);

        Assert.Equal(expectedUrl, actualUrl);
    }

    [Fact]
    public async Task GetLeaguesAsync_ReturnsLeagues()
    {
        var server = new MasterServerTMUF();
        
        var leagues = await server.GetLeaguesAsync();

        Assert.NotEmpty(leagues);
    }

    [Fact]
    public async Task GetLadderPlayerRankingsAsync_ReturnsPlayers()
    {
        var server = new MasterServerTMUF();

        var rankings = await server.GetLadderPlayerRankingsAsync();

        Assert.NotEqual(0, rankings.Count);
        Assert.NotEmpty(rankings.Players);
    }

    [Fact]
    public async Task GetLadderLeagueRankingsAsync_ReturnsPlayers()
    {
        var server = new MasterServerTMUF();

        var rankings = await server.GetLadderLeagueRankingsAsync();

        Assert.NotEqual(0, rankings.Count);
        Assert.NotEmpty(rankings.Leagues);
    }
}
