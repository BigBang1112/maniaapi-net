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
}
