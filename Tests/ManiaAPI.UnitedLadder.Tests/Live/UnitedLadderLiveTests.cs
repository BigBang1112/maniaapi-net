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

    [Test]
    public async Task GetPlayerAsync_UnknownLogin_IsNotFound()
    {
        using var ul = new UnitedLadder();

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => ul.GetPlayerAsync($"nsl{Guid.NewGuid():N}"[..12]));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}
