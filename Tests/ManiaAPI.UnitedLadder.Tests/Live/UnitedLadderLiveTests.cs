namespace ManiaAPI.UnitedLadder.Tests.Live;

[LiveTest]
public class UnitedLadderLiveTests
{
    [Test]
    public async Task GetPlayerAsync_ReturnsPlayer()
    {
        using var ul = new UnitedLadder();

        var player = await ul.GetPlayerAsync("bigbang1112");

        await Assert.That(player.Login).IsEqualTo("bigbang1112");
    }
}
