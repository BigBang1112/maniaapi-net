using ManiaAPI.Xml.MP3;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Unit;

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
}
