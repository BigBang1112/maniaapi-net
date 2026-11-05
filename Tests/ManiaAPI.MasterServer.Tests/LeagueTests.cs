namespace ManiaAPI.MasterServer.Tests;

public class LeagueTests
{
    [Test]
    public async Task GetFullPath_JoinsPathAndName()
    {
        var league = new League("Czech republic", "World|Europe", "");

        var fullPath = league.GetFullPath();

        await Assert.That(fullPath).IsEqualTo("World|Europe|Czech republic");
    }
}
