using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMT;
using ManiaAPI.MasterServer.TMUF;

namespace ManiaAPI.MasterServer.Tests.Live;

[Category("Live"), LiveTest, Retry(2)]
[NotInParallel("MasterServer")]
public class MasterServerLiveTests
{
    [Test]
    public async Task MP4_InitServer_ReturnsMasterServers()
    {
        using var server = new InitServerMP4();

        var waitingParams = await server.GetWaitingParamsAsync();

        await Assert.That(waitingParams.MasterServers).IsNotEmpty();
    }

    [Test]
    public async Task MP4_MasterServer_PlayerLoginAndLeagues()
    {
        using var server = new MasterServerMP4();
        // Player infos are only available on the second relay.
        using var relay02 = new MasterServerMP4(MasterServerMP4.DefaultUrlRelay02);

        var player = await relay02.GetPlayerInfosAsync("bigbang1112");
        var login = await server.CheckLoginAsync("bigbang1112");
        var leagues = await server.GetLeaguesAsync();

        await Assert.That(player.Login).IsEqualTo("bigbang1112");
        await Assert.That(login.Exists).IsTrue();
        await Assert.That(leagues).IsNotEmpty();
    }

    [Test]
    public async Task MP4_MasterServer_LeaderboardSummaries()
    {
        using var server = new MasterServerMP4();

        var campaign = await server.GetCampaignLeaderBoardSummariesAsync("TMValley@nadeo", [new(), new(Zone: "World|Europe")]);
        var maps = await server.GetMapLeaderBoardSummariesAsync("TMValley@nadeo", [new("JNDnboltOprJ19O2d2OQCJrC1Sk")]);

        await Assert.That(campaign[0].HighScores).IsNotEmpty();
        await Assert.That(maps[0].HighScores).IsNotEmpty();
    }

    [Test]
    public async Task TMUF_MasterServer_PlayerLoginAndLeagues()
    {
        using var server = new MasterServerTMUF();

        var player = await server.GetPlayerInfosAsync("bigbang1112");
        var login = await server.CheckLoginAsync("bigbang1112");
        var leagues = await server.GetLeaguesAsync();

        await Assert.That(player.Login).IsEqualTo("bigbang1112");
        await Assert.That(login.Exists).IsTrue();
        await Assert.That(leagues).IsNotEmpty();
    }

    [Test]
    public async Task TMUF_MasterServer_Ladder()
    {
        using var server = new MasterServerTMUF();

        var players = await server.GetLadderPlayerRankingsAsync();
        var achievements = await server.GetPlayerAchievementsAsync("bigbang1112");

        await Assert.That(players.Players).IsNotEmpty();
        await Assert.That(achievements.Maps).IsNotEmpty();
    }

    [Test]
    [Arguments(Platform.PC)]
    [Arguments(Platform.XB1)]
    [Arguments(Platform.PS4)]
    public async Task TMT_InitAndMasterServer_ReturnLeagues(Platform platform)
    {
        using var initServer = new InitServerTMT(platform);
        var waitingParams = await initServer.GetWaitingParamsAsync();

        using var masterServer = new MasterServerTMT(waitingParams.MasterServers[0]);
        var leagues = await masterServer.GetLeaguesAsync();

        await Assert.That(leagues).IsNotEmpty();
    }
}
