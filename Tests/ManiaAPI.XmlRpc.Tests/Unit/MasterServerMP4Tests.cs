using ManiaAPI.XmlRpc.MP4;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.XmlRpc.Tests.Unit;

public class MasterServerMP4Tests
{
    [Fact]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers()
    {
        var server = new InitServerMP4();

        var waitingParams = await server.GetWaitingParamsAsync();

        Assert.NotEmpty(waitingParams.MasterServers);
    }

    [Fact]
    public async Task ValidateAsync()
    {
        var server = new MasterServerMP4();

        await server.ValidateAsync();
    }

    [Fact]
    public async Task GetLeaguesAsync_ReturnsLeagues()
    {
        var server = new MasterServerMP4();

        var leagues = await server.GetLeaguesAsync();

        Assert.NotEmpty(leagues);
    }

    [Fact]
    public async Task GetCampaignLeaderBoardSummariesResponseAsync_ReturnsSummaries()
    {
        var server = new MasterServerMP4();

        var summaries = await server.GetCampaignLeaderBoardSummariesAsync("TMValley@nadeo",
            [new(), new(Zone: "World|Europe")]);

        Assert.NotEmpty(summaries);
    }

    [Fact]
    public async Task GetMapLeaderBoardSummariesResponseAsync_ReturnsSummaries()
    {
        var server = new MasterServerMP4();

        var summaries = await server.GetMapLeaderBoardSummariesAsync("TMValley@nadeo",
            [new("JNDnboltOprJ19O2d2OQCJrC1Sk"), new("f60UDPlW2mqfbhwCGUuclCIYQj2")]);

        Assert.NotEmpty(summaries);
    }
}
