namespace ManiaAPI.MasterServer.Tests;

public class MasterServerInfoTests
{
    [Test]
    [Arguments(true, "https://relay01.example.com:4443/game/request.php")]
    [Arguments(false, "http://relay01.example.com:8080/game/request.php")]
    public async Task GetUri_UsesPortOfScheme(bool useHttps, string expectedUri)
    {
        var info = new MasterServerInfo("Relay 01", "relay01.example.com", "game", 4443, 8080);

        var uri = info.GetUri(useHttps);

        await Assert.That(uri.ToString()).IsEqualTo(expectedUri);
    }
}
