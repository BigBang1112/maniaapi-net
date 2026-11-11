namespace ManiaAPI.NadeoAPI.Tests.Live;

/// <summary>
/// Requests that work with both kinds of account, so each one runs once per <see cref="NadeoAccount"/>.
/// </summary>
[LiveTest]
[NotInParallel("Nadeo")]
[Arguments(NadeoAccount.DedicatedServer)]
[Arguments(NadeoAccount.ServiceAccount)]
public class NadeoLiveTests(NadeoAccount account)
{
    private static readonly Guid[] AccountIds =
    [
        Guid.Parse("4a983dc3-a938-4564-8a1c-8be20779f069"),
        Guid.Parse("5dda004f-add9-43ad-b9cd-7a90101aa8f2")
    ];

    private static readonly Guid[] MapIds =
    [
        Guid.Parse("ed53486f-a80d-419b-b073-54bb8944d502"),
        Guid.Parse("e8a02ba9-f418-463d-8a83-ac5fc465ca8a")
    ];

    [Test]
    public async Task NadeoServices_MapsAndRecords()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        var mapInfos = await ns.GetMapInfosAsync(MapIds);
        var mapInfoByUid = await ns.GetMapInfoAsync(mapInfos[0].MapUid);
        var records = await ns.GetMapRecordsAsync(AccountIds, MapIds);
        var recordsById = await ns.GetMapRecordsByIdsAsync(records.Select(x => x.MapRecordId));

        await Assert.That(mapInfos.Select(x => x.MapId)).IsEquivalentTo(MapIds);
        await Assert.That(mapInfoByUid?.MapId).IsEqualTo(mapInfos[0].MapId);
        await Assert.That(records).IsNotEmpty();
        await Assert.That(recordsById.Select(x => x.MapRecordId)).IsEquivalentTo(records.Select(x => x.MapRecordId));
    }

    [Test]
    public async Task NadeoServices_AccountsAndZones()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        var zones = await ns.GetZonesAsync();
        var playerZones = await ns.GetPlayerZonesAsync(AccountIds);
        var webIdentities = await ns.GetPlayerWebIdentitiesAsync(AccountIds);
        var routes = await ns.GetApiRoutesAsync(ApiUsage.Server);

        await Assert.That(zones).IsNotEmpty();
        await Assert.That(playerZones.Select(x => x.AccountId)).IsEquivalentTo(AccountIds);
        await Assert.That(webIdentities).IsNotEmpty();
        await Assert.That(routes).IsNotEmpty();
    }

    [Test]
    public async Task NadeoLiveServices_CampaignsAndLeaderboards()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var campaigns = await nls.GetSeasonalCampaignsAsync(1);
        var campaign = campaigns.CampaignList[0];
        var mapUid = campaign.Playlist[0].MapUid;

        var mapInfo = await nls.GetMapInfoAsync(mapUid);
        var top = await nls.GetTopLeaderboardAsync(mapUid, campaign.SeasonUid, length: 5);
        var totds = await nls.GetTrackOfTheDaysAsync(1);

        await Assert.That(mapInfo?.Uid).IsEqualTo(mapUid);
        await Assert.That(top.Top.Top).IsNotEmpty();
        await Assert.That(totds.MonthList).IsNotEmpty();
    }

    [Test]
    public async Task NadeoLiveServices_Clubs()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var clubs = await nls.GetClubsAsync(1);
        var club = await nls.GetClubAsync(clubs.ClubList[0].Id);
        var members = await nls.GetClubMembersAsync(club.Id, 5);

        await Assert.That(club.Id).IsEqualTo(clubs.ClubList[0].Id);
        await Assert.That(members.ClubMemberList).IsNotEmpty();
    }

    [Test]
    public async Task NadeoMeetServices_CompetitionsAndMatchmaking()
    {
        var nms = await LiveNadeo.GetMeetServicesAsync(account);

        var competitions = await nms.GetCompetitionsAsync(1);
        var competition = await nms.GetCompetitionAsync(competitions[0].LiveId);
        var summary = await nms.GetMatchmakingSummaryAsync();
        var rankings = await nms.GetMatchmakingRankingsAsync(summary.Ranked3v3Id, 5);

        await Assert.That(competition.LiveId).IsEqualTo(competitions[0].LiveId);
        await Assert.That(rankings.Results).IsNotEmpty();
    }

    [Test]
    public async Task NadeoServices_OwnAccountRecords()
    {
        // Own records, because dedicated server accounts are forbidden from reading other accounts' records.
        var ns = await LiveNadeo.GetServicesAsync(account);
        var nls = await LiveNadeo.GetLiveServicesAsync(account);
        var accountId = ns.Handler.JWT?.Subject ?? throw new InvalidOperationException("The token has no account ID.");
        var seasonId = Guid.Parse((await nls.GetSeasonalCampaignsAsync(1)).CampaignList[0].SeasonUid);

#pragma warning disable CS0618 // Obsolete but still public, so it still needs to work.
        var records = await ns.GetAccountRecordsAsync(accountId);
#pragma warning restore CS0618
        var recordsByMapIds = await ns.GetAccountRecordsByMapIdsAsync(accountId, MapIds);
        var recordsByMapIdsWithOffset = await ns.GetAccountRecordsByMapIdsAsync(accountId, MapIds, gamemode: null, offset: 0);
        var recordsBySeasonIds = await ns.GetAccountRecordsBySeasonIdsAsync(accountId, [seasonId]);
        var recordsBySeasonIdsWithOffset = await ns.GetAccountRecordsBySeasonIdsAsync(accountId, [seasonId], gamemode: null, offset: 0);

        await Assert.That(records.Select(x => x.AccountId)).DoesNotContain(x => x != accountId);
        await Assert.That(recordsByMapIds.Select(x => x.AccountId)).DoesNotContain(x => x != accountId);
        await Assert.That(recordsByMapIdsWithOffset.Count).IsEqualTo(recordsByMapIds.Count);
        await Assert.That(recordsBySeasonIds.Select(x => x.AccountId)).DoesNotContain(x => x != accountId);
        await Assert.That(recordsBySeasonIdsWithOffset.Count).IsEqualTo(recordsBySeasonIds.Count);
    }

    [Test]
    public async Task NadeoServices_Players()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        var playerZone = await ns.GetPlayerZoneAsync(AccountIds[0]);
        var webIdentity = await ns.GetPlayerWebIdentityAsync(AccountIds[0]);

        await Assert.That(playerZone?.AccountId).IsEqualTo(AccountIds[0]);
        await Assert.That(webIdentity?.AccountId).IsEqualTo(AccountIds[0]);
    }

    [Test]
    public async Task NadeoServices_MapInfosByUid()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        var mapInfos = await ns.GetMapInfosAsync(MapIds);
        var mapInfosByUid = await ns.GetMapInfosAsync(mapInfos.Select(x => x.MapUid));
        var mapInfoById = await ns.GetMapInfoAsync(MapIds[0]);

        await Assert.That(mapInfosByUid.Select(x => x.MapId)).IsEquivalentTo(MapIds);
        await Assert.That(mapInfoById?.MapId).IsEqualTo(MapIds[0]);
    }

    [Test]
    public async Task NadeoLiveServices_OfficialCampaigns()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

#pragma warning disable CS0618 // Obsolete but still public, so it still needs to work.
        var weekly = await nls.GetWeeklyCampaignsAsync(1);
#pragma warning restore CS0618
        var weeklyShort = await nls.GetWeeklyShortCampaignsAsync(1);
        var weeklyGrand = await nls.GetWeeklyGrandCampaignsAsync(1);
        var cupOfTheWeek = await nls.GetCupOfTheWeekCampaignsAsync(1);
        var maniapubs = await nls.GetActiveManiapubsAsync();

        await Assert.That(weekly.CampaignList).IsNotEmpty();
        await Assert.That(weeklyShort.CampaignList).IsNotEmpty();
        await Assert.That(weeklyGrand.CampaignList).IsNotEmpty();
        await Assert.That(cupOfTheWeek.CampaignList).IsNotEmpty();
        await Assert.That(maniapubs).IsNotNull();
    }

    [Test]
    public async Task NadeoLiveServices_TrackOfTheDay()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var totds = await nls.GetTrackOfTheDaysAsync(1);
        var mapUid = totds.MonthList[0].Days.First(x => !string.IsNullOrEmpty(x.MapUid)).MapUid;

        var info = await nls.GetTrackOfTheDayInfoAsync(mapUid);

        await Assert.That(info.TotdMaps).Contains(mapUid);
    }

    [Test]
    public async Task NadeoLiveServices_Leaderboards()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var campaign = (await nls.GetSeasonalCampaignsAsync(1)).CampaignList[0];
        var mapUid = campaign.Playlist[0].MapUid;
        var groupUid = campaign.SeasonUid;

        var mapInfos = await nls.GetMapInfosAsync(campaign.Playlist.Select(x => x.MapUid));
        var medals = await nls.GetMapMedalRecordsAsync(mapUid, groupUid);
        var top = await nls.GetTopLeaderboardAsync(mapUid, groupUid, length: 5);
        var best = top.Top.Top[0];
        var zoneTop = await nls.GetTopLeaderboardAsync(mapUid, groupUid, best.ZoneId);
        var positions = await nls.GetLeaderboardPositionByTimeAsync(mapUid, groupUid, best.Score.TotalMilliseconds);
        var surrounding = await nls.GetSurroundingRecordsAsync(groupUid, mapUid, 1, 1, score: best.Score.TotalMilliseconds, onlyWorld: true);
        var campaignLeaderboard = await nls.GetCampaignLeaderboardAsync(groupUid);
        var seasonRankings = await nls.GetPlayerSeasonRankingsAsync(best.AccountId, groupUid);
        var trophyRankings = await nls.GetPlayerTrophyRankingsAsync(AccountIds);

        await Assert.That(mapInfos.Select(x => x.Uid)).IsEquivalentTo(campaign.Playlist.Select(x => x.MapUid));
        await Assert.That(medals.MapUid).IsEqualTo(mapUid);
        await Assert.That(zoneTop.Tops.SelectMany(x => x.Top)).IsNotEmpty();
        await Assert.That(positions).IsNotEmpty();
        await Assert.That(surrounding.Tops.SelectMany(x => x.Top)).IsNotEmpty();
        await Assert.That(campaignLeaderboard.Tops).IsNotEmpty();
        await Assert.That(seasonRankings.Zones).IsNotEmpty();
        await Assert.That(trophyRankings.Rankings.Select(x => x.AccountId)).IsEquivalentTo(AccountIds);
    }

    [Test]
    public async Task NadeoLiveServices_ClubCampaigns()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var clubCampaign = (await nls.GetClubCampaignsAsync(1)).ClubCampaignList[0];
        var clubId = clubCampaign.ClubId;

        var campaign = await nls.GetClubCampaignAsync(clubId, clubCampaign.CampaignId);
        var activities = await nls.GetClubActivitiesAsync(clubId, 5);

        await Assert.That(campaign.CampaignId).IsEqualTo(clubCampaign.CampaignId);
        await Assert.That(activities.ActivityList).IsNotEmpty();

        var groupUid = campaign.Campaign.LeaderboardGroupUid ?? campaign.Campaign.SeasonUid;
        var clubCampaignLeaderboard = await nls.GetClubCampaignLeaderboardAsync(groupUid, clubId);
        var clubTop = await nls.GetClubTopLeaderboardAsync(groupUid, campaign.Campaign.Playlist[0].MapUid, clubId);

        await Assert.That(clubCampaignLeaderboard.ClubId).IsEqualTo(clubId);
        await Assert.That(clubTop.ClubId).IsEqualTo(clubId);
    }

    [Test]
    public async Task NadeoLiveServices_ClubActivities()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var room = (await nls.GetClubRoomsAsync(1)).ClubRoomList[0];
        var roomById = await nls.GetClubRoomAsync(room.ClubId, room.ActivityId);

        var bucket = (await nls.GetClubBucketsAsync(ClubBucketType.MapUpload, 1)).ClubBucketList[0];
        var bucketById = await nls.GetClubBucketAsync(bucket.ClubId, bucket.Id);

        var mapReviews = await nls.GetClubMapReviewRoomsAsync(1);

        var clubCompetitions = await nls.GetClubCompetitionsAsync(1);

        await Assert.That(roomById.ActivityId).IsEqualTo(room.ActivityId);
        await Assert.That(bucketById.Id).IsEqualTo(bucket.Id);
        await Assert.That(mapReviews.ClubMapReviewList).IsNotEmpty();
        await Assert.That(clubCompetitions.ClubCompetitionList).IsNotEmpty();
    }

    [Test]
    public async Task NadeoLiveServices_ClubMember()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var club = (await nls.GetClubsAsync(1)).ClubList[0];
        var member = (await nls.GetClubMembersAsync(club.Id, 1)).ClubMemberList[0];

        var memberById = await nls.GetClubMemberAsync(club.Id, member.AccountId);

        await Assert.That(memberById.AccountId).IsEqualTo(member.AccountId);
    }

    [Test]
    public async Task NadeoLiveServices_OwnAccount()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var campaign = (await nls.GetSeasonalCampaignsAsync(1)).CampaignList[0];

        var clubPlayerInfo = await nls.GetClubPlayerInfoAsync();
        var myClubs = await nls.GetMyClubsAsync(5);
        var serverAccounts = await nls.GetDedicatedServerAccountsAsync();
        var uploadedMaps = await nls.GetUploadedMapsAsync(5);
        var favoriteMaps = await nls.GetFavoriteMapsAsync(5);
        var ownTrophyRanking = await nls.GetOwnTrophyRankingAsync();
        var playerRecords = await nls.GetPlayerRecordsAsync(campaign.Playlist.Select(x => x.MapUid));
        var mapReviewWaitingTime = await nls.GetMapReviewWaitingTimeAsync("totd");

        await Assert.That(clubPlayerInfo).IsNotNull();
        await Assert.That(myClubs.ClubList).IsNotNull();
        await Assert.That(serverAccounts.PlayerServerAccount).IsNotNull();
        await Assert.That(uploadedMaps.MapList).IsNotNull();
        await Assert.That(favoriteMaps.MapList).IsNotNull();
        await Assert.That(ownTrophyRanking).IsNotNull();
        await Assert.That(playerRecords).IsNotNull();
        await Assert.That(mapReviewWaitingTime).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task NadeoMeetServices_CupOfTheDay()
    {
        var nms = await LiveNadeo.GetMeetServicesAsync(account);

        var current = await nms.GetCurrentCupOfTheDayAsync();
        var cotd = await GetFinishedCupOfTheDayAsync(nms);
        var competitionId = cotd.Competition.LiveId;

        var leaderboard = await nms.GetCompetitionLeaderboardAsync(competitionId, 5);
        var participants = await nms.GetCompetitionParticipantsAsync(competitionId, 5);
        var rounds = await nms.GetCompetitionRoundsAsync(competitionId);
        var teams = await nms.GetCompetitionTeamsAsync(competitionId);
        var playerMatches = await nms.GetCompetitionPlayerMatchesAsync(competitionId, leaderboard[0].Participant);

        await Assert.That(current).IsNotNull();
        await Assert.That(leaderboard).IsNotEmpty();
        await Assert.That(participants).IsNotEmpty();
        await Assert.That(rounds).IsNotEmpty();
        await Assert.That(teams).IsNotNull();
        await Assert.That(playerMatches).IsNotEmpty();

        var match = (await nms.GetCompetitionRoundMatchesAsync(rounds[0].Id.ToString(), 1)).Matches[0];
        var matchResults = await nms.GetCompetitionMatchResultsAsync(match.ClubMatchLiveId, 5);
        var matchInfo = await nms.GetMatchAsync(match.ClubMatchLiveId);
        var matchTeams = await nms.GetMatchTeamsAsync(match.ClubMatchLiveId);
        var matchParticipants = await nms.GetMatchParticipantsAsync(match.ClubMatchLiveId);

        await Assert.That(matchResults.MatchLiveId).IsEqualTo(match.ClubMatchLiveId);
        await Assert.That(matchInfo.LiveId).IsEqualTo(match.ClubMatchLiveId);
        await Assert.That(matchParticipants).IsNotEmpty();
        await Assert.That(matchTeams).IsNotNull();
    }

    [Test]
    public async Task NadeoMeetServices_Challenges()
    {
        var nms = await LiveNadeo.GetMeetServicesAsync(account);
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var challenges = await nms.GetChallengesAsync(1);
        var cotd = await GetFinishedCupOfTheDayAsync(nms);
        var challengeId = cotd.Challenge.Id.ToString();

        var challenge = await nms.GetChallengeAsync(challengeId);
        var leaderboard = await nms.GetChallengeLeaderboardAsync(challengeId, 5);

        var totdDay = (await nls.GetTrackOfTheDaysAsync(2)).MonthList
            .SelectMany(x => x.Days)
            .First(x => x.StartTimestamp <= cotd.StartDate && cotd.StartDate < x.EndTimestamp);
        var mapRecords = await nms.GetChallengeMapRecordsAsync(challengeId, totdDay.MapUid, 5);

        await Assert.That(challenges).IsNotEmpty();
        await Assert.That(challenge.Id).IsEqualTo(cotd.Challenge.Id);
        await Assert.That(leaderboard.Results).IsNotEmpty();
        await Assert.That(mapRecords).IsNotEmpty();
    }

    [Test]
    public async Task NadeoMeetServices_Matchmaking()
    {
        var nms = await LiveNadeo.GetMeetServicesAsync(account);

        var summary = await nms.GetMatchmakingSummaryAsync();
        var players = (await nms.GetMatchmakingRankingsAsync(summary.Ranked3v3Id, 2)).Results.Select(x => x.Player).ToList();

        var ranks = await nms.GetMatchmakingPlayerRanksAsync(summary.Ranked3v3Id, players);
        var progressions = await nms.GetMatchmakingPlayerProgressionsAsync(summary.Ranked3v3Id, players);
        var divisions = await nms.GetMatchmakingDivisionsAsync(summary.Ranked3v3Id);
        var status = await nms.GetMatchmakingPlayerStatusAsync(summary.Ranked3v3Id);
        var superRoyalStatus = await nms.GetSuperRoyalStatusAsync();
        var superRoyalStatistics = await nms.GetSuperRoyalStatisticsAsync();
        var myClubCompetitions = await nms.GetMyClubCompetitionsAsync(5);

        await Assert.That(ranks.Results.Select(x => x.Player)).IsEquivalentTo(players);
        await Assert.That(progressions.Progressions).IsNotEmpty();
        await Assert.That(divisions.Divisions).IsNotEmpty();
        await Assert.That(status).IsNotNull();
        await Assert.That(myClubCompetitions).IsNotNull();
    }

    private static async Task<CupOfTheDay> GetFinishedCupOfTheDayAsync(NadeoMeetServices nms)
    {
        // Each cup also has empty per-platform editions (pc, psn, xbl, luna) next to the crossplay one with the players.
        var cups = await nms.GetCupsOfTheDayAsync(CupOfTheDayType.COTD, 30);

        return cups.COTDs.Where(x => x.EndDate < DateTimeOffset.UtcNow && x.Competition.NbPlayers > 0).MaxBy(x => x.EndDate)
            ?? throw new InvalidOperationException("No finished cup of the day found.");
    }
}
