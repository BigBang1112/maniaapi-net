using ManiaAPI.Xml.MP4;
using ManiaAPI.Xml.TMUF;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Unit;

public class InitServerMP4Tests
{
    [Fact]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers()
    {
        var server = new InitServerMP4();

        var waitingParams = await server.GetWaitingParamsAsync();

        Assert.NotEmpty(waitingParams.MasterServers);
    }
}
