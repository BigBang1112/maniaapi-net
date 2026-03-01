using ManiaAPI.Xml.MP3;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Unit;

public class MasterServerMP3Tests
{
    [Fact]
    public async Task TestAsync()
    {
        var server = new MasterServerMP3();

        var response = await server.TestAsync();

        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetPlayerInfos_ReturnsPlayerInfos()
    {
        var server = new MasterServerMP3();

        var playerInfos = await server.GetPlayerInfosAsync("bigbang1112");

        Assert.NotNull(playerInfos);
    }

    [Fact]
    public async Task CheckLoginAsync_ReturnsExistence()
    {
        var server = new MasterServerMP3();

        var result = await server.CheckLoginAsync("bigbang1112");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAccountFromSteamUserAsync_ReturnsLogin()
    {
        var server = new MasterServerMP3();

        var login = await server.GetAccountFromSteamUserAsync(76561198060959523);

        Assert.NotNull(login);
    }

    [Fact]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers()
    {
        var server = new MasterServerMP3();

        var waitingParams = await server.GetWaitingParamsAsync("bigbang1112");

        Assert.NotEmpty(waitingParams.MasterServers);
    }

    [Fact]
    public async Task GetLeaguesAsync_ReturnsLeagues()
    {
        var server = new MasterServerMP3();

        var leagues = await server.GetLeaguesAsync();

        Assert.NotEmpty(leagues);
    }

    [Fact]
    public async Task GetMultiplayerLeaderBoardSummariesAsync_ReturnsSummaries()
    {
        var server = new MasterServerMP3();

        var summaries = await server.GetMultiplayerLeaderBoardSummariesAsync("TMValley", "World", "World|Europe");

        Assert.NotEmpty(summaries);
    }
}
