using ManiaAPI.XmlRpc.TMUF;
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
}
