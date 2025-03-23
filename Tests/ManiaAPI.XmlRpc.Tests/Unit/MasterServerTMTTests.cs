using ManiaAPI.XmlRpc.TMT;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.XmlRpc.Tests.Unit;

public class MasterServerTMTTests
{
    [Theory]
    [InlineData(Platform.PC)]
    [InlineData(Platform.XB1)]
    [InlineData(Platform.PS4)]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers(Platform platform)
    {
        var server = new InitServerTMT(platform);

        var waitingParams = await server.GetWaitingParamsAsync();

        Assert.NotEmpty(waitingParams.MasterServers);
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
