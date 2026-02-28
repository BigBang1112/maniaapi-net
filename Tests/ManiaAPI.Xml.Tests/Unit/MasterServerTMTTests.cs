using ManiaAPI.Xml.MP4;
using ManiaAPI.Xml.TMT;
using ManiaAPI.Xml.TMUF;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Unit;

public class MasterServerTMTTests
{
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
