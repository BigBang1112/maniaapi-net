using ManiaAPI.MasterServer.MP3;
using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMT;
using ManiaAPI.MasterServer.TMUF;
using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.Tests.Live;

[LiveTest]
[NotInParallel("MasterServer")]
public class MasterServerLiveTests
{
    private const string TmtMapUid = "XUuBzeBgKAx0m6MMstYeTuWZGdb";

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

    [Test]
    public async Task MP3_InitAndMasterServer_ReturnLeagues()
    {
        using var initServer = new InitServerMP3();
        var waitingParams = await initServer.GetWaitingParamsAsync();

        using var masterServer = new MasterServerMP3();
        var leagues = await masterServer.GetLeaguesAsync();

        await Assert.That(waitingParams.MasterServers).IsNotEmpty();
        await Assert.That(leagues).IsNotEmpty();
    }

    [Test]
    public async Task MP4_MasterServer_TestAndWaitingParams()
    {
        using var server = new MasterServerMP4();

        _ = await server.TestAsync();
        var waitingParams = await server.GetWaitingParamsAsync("bigbang1112");

        await Assert.That(waitingParams.MasterServers).IsNotEmpty();
    }

    [Test]
    public async Task MP4_MasterServer_Leaderboards()
    {
        using var server = new MasterServerMP4();

        var campaign = await server.GetCampaignLeaderBoardAsync("TMValley@nadeo", count: 5);
        var map = await server.GetMapLeaderBoardAsync("TMValley@nadeo", "JNDnboltOprJ19O2d2OQCJrC1Sk", count: 5);
        var multiplayer = await server.GetMultiplayerLeaderBoardSummariesAsync("TMStadium@nadeo", "World");

        await Assert.That(campaign).IsNotEmpty();
        await Assert.That(map).IsNotEmpty();
        await Assert.That(multiplayer).IsNotEmpty();
    }

    [Test]
    public async Task MP4_MasterServer_AccountLookups()
    {
        using var server = new MasterServerMP4();

        var webIdentity = await server.GetWebIdentityFromManiaplanetLoginAsync("bigbang1112");
        var unknownWebIdentityLogin = await server.GetManiaplanetLoginFromWebIdentityAsync("uplay", Guid.Empty);
        var unknownSteamAccount = await server.GetAccountFromSteamUserAsync(0);

        await Assert.That(webIdentity?.Login).IsEqualTo("bigbang1112");
        await Assert.That(unknownWebIdentityLogin).IsNull();
        // The master server answers an unlinked Uplay profile with an error instead of an empty result.
        await Assert.ThrowsAsync<XmlRequestException>(() => server.GetAccountFromUplayUserAsync(Guid.Empty));
        await Assert.That(unknownSteamAccount).IsNull();
    }

    [Test]
    public async Task TMT_MasterServer_LeaderboardSummaries()
    {
        using var initServer = new InitServerTMT(Platform.PC);
        var waitingParams = await initServer.GetWaitingParamsAsync();
        using var server = new MasterServerTMT(waitingParams.MasterServers[0]);

        var campaigns = await server.GetCampaignLeaderBoardSummariesAsync("World", "World|Japan");
        var campaign = await server.GetCampaignLeaderBoardSummaryAsync();
        var maps = await server.GetMapLeaderBoardSummariesAsync(TmtMapUid, "World", "World|Japan");
        var map = await server.GetMapLeaderBoardSummaryAsync(TmtMapUid);

        await Assert.That(campaigns.Select(x => x.Zone)).IsEquivalentTo(["World", "World|Japan"]);
        await Assert.That(campaign.Scores).IsNotEmpty();
        await Assert.That(maps.Select(x => x.Zone)).IsEquivalentTo(["World", "World|Japan"]);
        await Assert.That(map.Scores).IsNotEmpty();
    }

    [Test]
    public async Task TMT_AggregatedMasterServer_LeaderboardSummaries()
    {
        var masterServers = ImmutableDictionary.CreateBuilder<Platform, MasterServerTMT>();

        foreach (var platform in Enum.GetValues<Platform>())
        {
            using var initServer = new InitServerTMT(platform);
            var waitingParams = await initServer.GetWaitingParamsAsync();
            masterServers[platform] = new MasterServerTMT(waitingParams.MasterServers[0]);
        }

        try
        {
            var server = new AggregatedMasterServerTMT(masterServers.ToImmutable());

            var campaigns = await server.GetCampaignLeaderBoardSummariesAsync("World", "World|Japan");
            var campaign = await server.GetCampaignLeaderBoardSummaryAsync();
            var maps = await server.GetMapLeaderBoardSummariesAsync(TmtMapUid, "World", "World|Japan");
            var map = await server.GetMapLeaderBoardSummaryAsync(TmtMapUid);

            await Assert.That(campaigns.Select(x => x.Zone)).IsEquivalentTo(["World", "World|Japan"]);
            await Assert.That(campaign.Timestamps.Keys).IsEquivalentTo(Enum.GetValues<Platform>());
            await Assert.That(maps.Select(x => x.Zone)).IsEquivalentTo(["World", "World|Japan"]);
            await Assert.That(map.Scores).IsNotEmpty();
        }
        finally
        {
            foreach (var masterServer in masterServers.Values)
            {
                masterServer.Dispose();
            }
        }
    }

    [Test]
    public async Task TMUF_MasterServer_LeagueRankingsAndCampaignScores()
    {
        using var server = new MasterServerTMUF();

        var leagueRankings = await server.GetLadderLeagueRankingsAsync();
        var campaignScores = await server.GetCampaignScoresAsync("Nations");

        await Assert.That(leagueRankings.Leagues).IsNotEmpty();
        await Assert.That(campaignScores?.Url).IsNotNull().And.IsNotEmpty();
    }

    [Test]
    public async Task TMUF_MasterServer_ScoresFiles()
    {
        // A small league keeps the downloads to a few hundred kilobytes.
        const string league = "World|Algeria";
        using var server = new MasterServerTMUF();

        var generalInfo = await server.FetchLatestGeneralScoresInfoAsync(league);
        var ladderInfo = await server.FetchLatestLadderScoresInfoAsync(league);
        var campaignInfo = await server.FetchLatestCampaignScoresInfoAsync("Nations", league);

        await Assert.That(generalInfo).IsNotNull();
        await Assert.That(ladderInfo).IsNotNull();
        await Assert.That(campaignInfo).IsNotNull();

        var generalModified = await server.FetchGeneralScoresDateTimeAsync(generalInfo!.Value.Number, league);
        var ladderModified = await server.FetchLadderScoresDateTimeAsync(ladderInfo!.Value.Number, league);
        var campaignModified = await server.FetchCampaignScoresDateTimeAsync("Nations", campaignInfo!.Value.Number, league);

        await Assert.That(generalModified).IsEqualTo(generalInfo.Value.LastModified);
        await Assert.That(ladderModified).IsEqualTo(ladderInfo.Value.LastModified);
        await Assert.That(campaignModified).IsEqualTo(campaignInfo.Value.LastModified);

        var general = await server.DownloadLatestGeneralScoresAsync(league);
        var ladder = await server.DownloadLatestLadderScoresAsync(league);
        var campaign = await server.DownloadLatestCampaignScoresAsync("Nations", league);

        await Assert.That(general).IsNotNull();
        await Assert.That(ladder).IsNotNull();
        await Assert.That(campaign).IsNotNull();
    }
}
