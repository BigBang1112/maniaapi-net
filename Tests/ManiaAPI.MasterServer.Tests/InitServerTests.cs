using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMT;

namespace ManiaAPI.MasterServer.Tests;

public class InitServerTests
{
    private const string Url = "https://init.example.com/game/request.php";
    private const string Login = "bigbang1112";

    private const string WaitingParamsContent =
        "<wt>30</wt><wm>Busy</wm>" +
        "<ms><b>Relay 01</b><c>relay01.example.com</c><d>game</d><e>4443</e><f>8080</f></ms>" +
        "<ms><b>Relay 02</b><c>relay02.example.com</c><d>game</d></ms>";

    [Test]
    public async Task GetWaitingParamsAsync_ReturnsParsedWaitingParams()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetWaitingParams", WaitingParamsContent));
        using var server = new InitServerMP4(fake.CreateClient(Url));

        var waitingParams = await server.GetWaitingParamsAsync();

        await Assert.That(waitingParams.WaitingQueueDuration).IsEqualTo(30);
        await Assert.That(waitingParams.WaitingQueueMessage).IsEqualTo("Busy");
        await Assert.That(waitingParams.MasterServers).IsEquivalentTo(
        [
            new MasterServerInfo("Relay 01", "relay01.example.com", "game", 4443, 8080),
            new MasterServerInfo("Relay 02", "relay02.example.com", "game", 443, 80)
        ]);
    }

    [Test]
    public async Task GetWaitingParamsAsync_WithLogin_SendsAuthorXml()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetWaitingParams", ""));
        using var server = new InitServerMP4(fake.CreateClient(Url));

        await server.GetWaitingParamsAsync(Login);

        await Assert.That(fake.Request.Body).Contains($"</game><author><login>{Login}</login></author><request><name>GetWaitingParams</name>");
    }

    [Test]
    public async Task GetWaitingParamsAsync_WithoutLogin_SendsNoAuthorXml()
    {
        var fake = MasterServerXml.Handler(MasterServerXml.Success("GetWaitingParams", ""));
        using var server = new InitServerMP4(fake.CreateClient(Url));

        await server.GetWaitingParamsAsync();

        await Assert.That(fake.Request.Body).DoesNotContain("<author>");
    }

    [Test]
    [Arguments(Platform.PC, "https://init-pc.turbo.trackmania.com/game/request.php")]
    [Arguments(Platform.XB1, "https://init-xb1.turbo.trackmania.com/game/request.php")]
    [Arguments(Platform.PS4, "https://init-ps4.turbo.trackmania.com/game/request.php")]
    public async Task GetDefaultUrl_Platform_ReturnsInitServerOfPlatform(Platform platform, string expectedUrl)
    {
        var url = InitServerTMT.GetDefaultUrl(platform);

        await Assert.That(url).IsEqualTo(expectedUrl);
    }
}
