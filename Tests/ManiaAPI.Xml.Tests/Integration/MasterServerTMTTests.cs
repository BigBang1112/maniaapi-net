using ManiaAPI.Xml.MP3;
using ManiaAPI.Xml.TMT;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Integration;

public class MasterServerTMTTests
{
    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task TestAsync(Platform platform)
    {
        var initServer = new InitServerTMT(platform);
        var waitingParams = await initServer.GetWaitingParamsAsync();

        var response = await initServer.TestAsync();

        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var response2 = await masterServer.TestAsync();

        Assert.NotNull(response);
        Assert.NotNull(response2);
    }

    [Fact]
    public async Task GetPlayerInfos_ReturnsPlayerInfos()
    {
        var initServer = new InitServerTMT(Platform.PC);
        var waitingParams = await initServer.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var playerInfos = await masterServer.GetPlayerInfosAsync("zojytyxy-pc56f3bdff95566");
        Assert.NotNull(playerInfos);
    }

    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers(Platform platform)
    {
        var server = new InitServerTMT(platform);
        var waitingParams = await server.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var waitingParams2 = await masterServer.GetWaitingParamsAsync();

        Assert.NotEmpty(waitingParams.MasterServers);
        Assert.NotEmpty(waitingParams2.MasterServers);
    }

    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task GetAccountFromSteamUserAsync_ReturnsLogin(Platform platform)
    {
        var server = new InitServerTMT(platform);
        var login1 = await server.GetAccountFromSteamUserAsync(76561198060959523);
        var waitingParams = await server.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var login2 = await masterServer.GetAccountFromSteamUserAsync(76561198060959523);

        Assert.Null(login1);
        Assert.Null(login2);
    }

    [Fact]
    public async Task GetWebIdentityFromManiaplanetLoginAsync_ReturnsWebIdentities()
    {
        var server = new InitServerTMT(Platform.PC);
        var identity1 = await server.GetWebIdentityFromManiaplanetLoginAsync("zojytyxy-pc56f3bdff95566");
        var waitingParams = await server.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var identity2 = await masterServer.GetWebIdentityFromManiaplanetLoginAsync("zojytyxy-pc56f3bdff95566");

        Assert.NotNull(identity1);
        Assert.NotNull(identity2);
    }

    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task GetLeaguesAsync_ReturnsLeagues(Platform platform)
    {
        var initServer = new InitServerTMT(platform);
        var waitingParams = await initServer.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var leagues = await masterServer.GetLeaguesAsync();

        Assert.NotEmpty(leagues);
    }

    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task CheckLoginAsync_ReturnsExistence(Platform platform)
    {
        var initServer = new InitServerTMT(platform);
        var waitingParams = await initServer.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var result = await masterServer.CheckLoginAsync("zojytyxy-pc56f3bdff95566");

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task GetCampaignLeaderBoardSummariesAsync_ReturnsSummaries(Platform platform)
    {
        var initServer = new InitServerTMT(platform);
        var waitingParams = await initServer.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var summaries = await masterServer.GetCampaignLeaderBoardSummariesAsync(["World", "World|Japan"]);

        Assert.NotEmpty(summaries);
    }

    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task GetMapLeaderBoardSummariesAsync_ReturnsSummaries(Platform platform)
    {
        var initServer = new InitServerTMT(platform);
        var waitingParams = await initServer.GetWaitingParamsAsync();
        var masterServer = new MasterServerTMT(waitingParams.MasterServers.First());

        var summaries = await masterServer.GetMapLeaderBoardSummariesAsync("jV1HYr_Vp3uijy8jgaC5TkHkRs", ["World", "World|Japan"]);

        Assert.NotEmpty(summaries);
    }
}
