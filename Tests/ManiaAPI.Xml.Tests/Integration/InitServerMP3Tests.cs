using ManiaAPI.Xml.MP3;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Integration;

public class InitServerMP3Tests
{
    [Fact]
    public async Task TestAsync()
    {
        var server = new InitServerMP3();

        var response = await server.TestAsync();

        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers()
    {
        var server = new InitServerMP3();

        var waitingParams = await server.GetWaitingParamsAsync();

        Assert.NotEmpty(waitingParams.MasterServers);
    }

    [Fact]
    public async Task GetAccountFromSteamUserAsync_ReturnsLogin()
    {
        var server = new InitServerMP3();

        var login = await server.GetAccountFromSteamUserAsync(76561198060959523);

        Assert.NotNull(login);
    }

    [Fact]
    public async Task GetWebIdentityFromManiaplanetLoginAsync_ReturnsWebIdentities()
    {
        var server = new InitServerMP3();

        var webIdentities = await server.GetWebIdentityFromManiaplanetLoginAsync("futurecat", "riolu", "bigbang1112");

        Assert.NotEmpty(webIdentities);
    }
}
