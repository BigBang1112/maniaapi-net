using ManiaAPI.Xml.MP4;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.Xml.Tests.Integration;

public class MasterServerMP4Tests
{
    [Fact]
    public async Task TestAsync()
    {
        var server = new MasterServerMP4();

        var response = await server.TestAsync();

        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetPlayerInfos_ReturnsPlayerInfos()
    {
        var server = new MasterServerMP4();

        var playerInfos = await server.GetPlayerInfosAsync("bigbang1112");

        Assert.NotNull(playerInfos);
    }

    [Fact]
    public async Task CheckLoginAsync_ReturnsExistence()
    {
        var server = new MasterServerMP4();

        var result = await server.CheckLoginAsync("bigbang1112");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAccountFromSteamUserAsync_ReturnsLogin()
    {
        var server = new MasterServerMP4();

        var login = await server.GetAccountFromSteamUserAsync(76561198060959523);

        Assert.NotNull(login);
    }

    [Fact]
    public async Task GetAccountFromUplayUserAsync_ReturnsLogin()
    {
        var server = new MasterServerMP4();

        var login = await server.GetAccountFromUplayUserAsync(Guid.Parse("0bca2165-693d-41de-b723-632966e8de74"));

        Assert.NotNull(login);
    }

    [Fact]
    public async Task GetWebIdentityFromManiaplanetLoginAsync_ReturnsWebIdentities()
    {
        var server = new MasterServerMP4();

        var webIdentities = await server.GetWebIdentityFromManiaplanetLoginAsync("futurecat", "riolu", "eddyey");

        Assert.NotEmpty(webIdentities);
    }

    [Fact]
    public async Task GetWaitingParamsAsync_ReturnsMasterServers()
    {
        var server = new MasterServerMP4();

        var waitingParams = await server.GetWaitingParamsAsync("bigbang1112");

        Assert.NotEmpty(waitingParams.MasterServers);
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

    [Fact]
    public async Task GetMultiplayerLeaderBoardSummariesAsync_ReturnsSummaries()
    {
        var server = new MasterServerMP4();

        var summaries = await server.GetMultiplayerLeaderBoardSummariesAsync("TMValley@nadeo", "World", "World|Europe");

        Assert.NotEmpty(summaries);
    }
}
