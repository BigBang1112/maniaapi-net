namespace ManiaAPI.NadeoAPI.Tests.Live;

/// <summary>
/// Every test runs once per <see cref="NadeoAccount"/>, so it's known which requests work with which account.
/// A request that only a service account may send is checked to be denied with a dedicated server account, see <see cref="AssertDeniedAsync"/>.
/// </summary>
[LiveTest]
[NotInParallel("NadeoAPI")]
[Arguments(NadeoAccount.DedicatedServer)]
[Arguments(NadeoAccount.ServiceAccount)]
public class NadeoLiveTests(NadeoAccount account)
{
    /// <summary>
    /// Two long-standing player accounts, picked because they have records on <see cref="MapIds"/>, club tags and trophy history.
    /// </summary>
    private static readonly Guid[] AccountIds =
    [
        Guid.Parse("4a983dc3-a938-4564-8a1c-8be20779f069"),
        Guid.Parse("5dda004f-add9-43ad-b9cd-7a90101aa8f2")
    ];

    /// <summary>
    /// Two maps of an official campaign, so they won't be deleted and both <see cref="AccountIds"/> have records on them.
    /// </summary>
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
        var mapInfosByUid = await ns.GetMapInfosAsync(mapInfos.Select(x => x.MapUid));
        var mapInfoById = await ns.GetMapInfoAsync(MapIds[0]);
        var mapInfoByUid = await ns.GetMapInfoAsync(mapInfos[0].MapUid);
        var records = await ns.GetMapRecordsAsync(AccountIds, MapIds);
        var recordsById = await ns.GetMapRecordsByIdsAsync(records.Select(x => x.MapRecordId));
        var recordById = await ns.GetMapRecordByIdAsync(records[0].MapRecordId);

        await Assert.That(mapInfos.Select(x => x.MapId)).IsEquivalentTo(MapIds);
        await Assert.That(mapInfos).DoesNotContain(x => string.IsNullOrEmpty(x.MapUid) || x.AuthorScore.TotalMilliseconds <= 0);
        await Assert.That(mapInfosByUid.Select(x => x.MapId)).IsEquivalentTo(MapIds);
        await Assert.That(mapInfoById?.MapId).IsEqualTo(MapIds[0]);
        await Assert.That(mapInfoByUid?.MapId).IsEqualTo(mapInfos[0].MapId);
        await Assert.That(records).IsNotEmpty();
        await Assert.That(records).DoesNotContain(x => !AccountIds.Contains(x.AccountId) || !MapIds.Contains(x.MapId));
        await Assert.That(recordsById.Select(x => x.MapRecordId)).IsEquivalentTo(records.Select(x => x.MapRecordId));
        await Assert.That(recordById.MapRecordId).IsEqualTo(records[0].MapRecordId);
        await Assert.That(recordById.RecordScore.Time.TotalMilliseconds).IsGreaterThan(0);
    }

    [Test]
    public async Task NadeoServices_AccountsAndZones()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        var zones = await ns.GetZonesAsync();
        var webIdentities = await ns.GetPlayerWebIdentitiesAsync(AccountIds);
        var webIdentity = await ns.GetPlayerWebIdentityAsync(AccountIds[0]);
        var serverRoutes = await ns.GetApiRoutesAsync(ApiUsage.Server);
        var clientRoutes = await ns.GetApiRoutesAsync(ApiUsage.Client);

        await Assert.That(zones).Contains(x => x.Name == "World" && x.ParentId is null);
        await Assert.That(webIdentities.Select(x => x.AccountId)).DoesNotContain(x => !AccountIds.Contains(x));
        await Assert.That(webIdentities).DoesNotContain(x => string.IsNullOrEmpty(x.Provider));
        await Assert.That(webIdentity?.AccountId).IsEqualTo(AccountIds[0]);
        // The lists overlap, but neither contains the other: a few routes are listed only for servers.
        await Assert.That(serverRoutes).IsNotEmpty();
        await Assert.That(serverRoutes.Values).DoesNotContain(x => string.IsNullOrEmpty(x.Method) || string.IsNullOrEmpty(x.Path));
        await Assert.That(clientRoutes).IsNotEmpty();
        await Assert.That(clientRoutes.Values).DoesNotContain(x => string.IsNullOrEmpty(x.Method) || string.IsNullOrEmpty(x.Path));
    }

    [Test]
    public async Task NadeoServices_PlayerZones()
    {
        // The Openplanet documentation says dedicated server accounts get 401 here, but they can read player zones.
        var ns = await LiveNadeo.GetServicesAsync(account);

        var playerZones = await ns.GetPlayerZonesAsync(AccountIds);
        var playerZone = await ns.GetPlayerZoneAsync(AccountIds[0]);

        await Assert.That(playerZones.Select(x => x.AccountId)).IsEquivalentTo(AccountIds);
        await Assert.That(playerZones).DoesNotContain(x => x.ZoneId == Guid.Empty);
        await Assert.That(playerZone?.AccountId).IsEqualTo(AccountIds[0]);
    }

    [Test]
    public async Task NadeoServices_OwnAccountRecords()
    {
        // A dedicated server account can send these requests, but it has no records of its own, so it gets none.
        var ns = await LiveNadeo.GetServicesAsync(account);
        var accountId = GetOwnAccountId(ns);
        var seasonId = Guid.Parse((await LiveNadeo.GetSeasonalCampaignAsync(account)).SeasonUid);

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
    public async Task NadeoServices_OtherAccountRecords_AreForbidden()
    {
        // Account records are only for the authenticated account itself, with either kind of account.
        var ns = await LiveNadeo.GetServicesAsync(account);

        await AssertDeniedAsync(() => ns.GetAccountRecordsByMapIdsAsync(AccountIds[0], MapIds), HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task NadeoServices_OwnAccountMaps()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);
        var accountId = GetOwnAccountId(ns);
        var mapUids = (await LiveNadeo.GetSeasonalCampaignAsync(account)).Playlist.Select(x => x.MapUid).ToList();

        if (account is NadeoAccount.DedicatedServer)
        {
            await AssertDeniedAsync(() => ns.GetMapsByAuthorAsync());
            await AssertDeniedAsync(() => ns.GetSubmittedMapsAsync());
            await AssertDeniedAsync(() => ns.GetFavoriteMapsByUidsAsync(mapUids));
            await AssertDeniedAsync(() => ns.GetFavoriteSkinsAsync(accountId));
            await AssertDeniedAsync(() => ns.GetMapVoteAsync(mapUids[0]));
            return;
        }

        var byAuthor = await ns.GetMapsByAuthorAsync();
        // The server redirects maps/by-submitter to maps/by-submitter/, and a redirect drops the Authorization header.
        var submitted = await ns.GetSubmittedMapsAsync();
        var favorites = await ns.GetFavoriteMapsByUidsAsync(mapUids);
        var favoriteSkins = await ns.GetFavoriteSkinsAsync(accountId);
        var vote = await ns.GetMapVoteAsync(mapUids[0]);

        await Assert.That(byAuthor.MapList).DoesNotContain(x => x.Author != accountId);
        await Assert.That(submitted.MapList).DoesNotContain(x => x.Submitter != accountId);
        await Assert.That(favorites.MapFavoriteList.Count).IsLessThanOrEqualTo(mapUids.Count);
        await Assert.That(favoriteSkins).DoesNotContain(x => x.AccountId != accountId || x.SkinId == Guid.Empty);
        await Assert.That(vote.MapUid).IsEqualTo(mapUids[0]);
        await Assert.That(vote.AccountId).IsEqualTo(accountId);
    }

    [Test]
    public async Task NadeoServices_Skins()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        if (account is NadeoAccount.DedicatedServer)
        {
            await AssertDeniedAsync(() => ns.GetSkinsByAccountIdsAsync(AccountIds));
            await AssertDeniedAsync(() => ns.GetSkinByAccountIdAsync(AccountIds[0]));
            return;
        }

        var skins = await ns.GetSkinsByAccountIdsAsync(AccountIds);
        await Assert.That(skins).IsNotEmpty();
        await Assert.That(skins.Select(x => x.AccountId)).DoesNotContain(x => !AccountIds.Contains(x));

        var skin = skins[0];
        var skinInfo = await ns.GetSkinInfoAsync(skin.SkinId);
        var skinByAccount = await ns.GetSkinByAccountIdAsync(skin.AccountId);

        await Assert.That(skinInfo?.SkinId).IsEqualTo(skin.SkinId);
        await Assert.That(skinInfo?.FileUrl).IsNotNull().And.IsNotEmpty();
        await Assert.That(skinByAccount?.AccountId).IsEqualTo(skin.AccountId);
    }

    [Test]
    public async Task NadeoServices_PlayerClubTags()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        if (account is NadeoAccount.DedicatedServer)
        {
            await AssertDeniedAsync(() => ns.GetPlayerClubTagsAsync(AccountIds));
            return;
        }

        var clubTags = await ns.GetPlayerClubTagsAsync(AccountIds);

        // Accounts without a club tag are left out of the response, but both AccountIds have one.
        await Assert.That(clubTags).IsNotEmpty();
        await Assert.That(clubTags.Select(x => x.AccountId)).DoesNotContain(x => !AccountIds.Contains(x));
        await Assert.That(clubTags).DoesNotContain(x => string.IsNullOrEmpty(x.ClubTag));
    }

    [Test]
    public async Task NadeoServices_PlayerTrophies()
    {
        var ns = await LiveNadeo.GetServicesAsync(account);

        if (account is NadeoAccount.DedicatedServer)
        {
            await AssertDeniedAsync(() => ns.GetPlayerTrophySummaryAsync(AccountIds[0]));
            await AssertDeniedAsync(() => ns.GetPlayerTrophyHistoryAsync(AccountIds[0], count: 5));
            return;
        }

        var summary = await ns.GetPlayerTrophySummaryAsync(AccountIds[0]);
        var history = await ns.GetPlayerTrophyHistoryAsync(AccountIds[0], count: 5);

        await Assert.That(summary.AccountId).IsEqualTo(AccountIds[0]);
        await Assert.That(summary.Points).IsGreaterThan(0);
        await Assert.That(history.Data).IsNotEmpty();
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
        var seasonal = await LiveNadeo.GetSeasonalCampaignAsync(account);

        await Assert.That(weekly.CampaignList).IsNotEmpty();
        await Assert.That(weeklyShort.CampaignList).IsNotEmpty();
        await Assert.That(weeklyGrand.CampaignList).IsNotEmpty();
        await Assert.That(cupOfTheWeek.CampaignList).IsNotEmpty();
        await Assert.That(weeklyShort.CampaignList[0].Playlist).IsNotEmpty();
        // There may be no maniapub running, but the ones that are must be readable.
        await Assert.That(maniapubs).DoesNotContain(x => x.CampaignUid == Guid.Empty || string.IsNullOrEmpty(x.Name));
        await Assert.That(seasonal.Name).IsNotEmpty();
        await Assert.That(seasonal.Playlist).IsNotEmpty();
    }

    [Test]
    public async Task NadeoLiveServices_TrackOfTheDay()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var totds = await nls.GetTrackOfTheDaysAsync(1);
        var mapUid = totds.MonthList[0].Days.First(x => !string.IsNullOrEmpty(x.MapUid)).MapUid;

        var info = await nls.GetTrackOfTheDayInfoAsync(mapUid);

        await Assert.That(totds.MonthList[0].Year).IsGreaterThanOrEqualTo(2020);
        await Assert.That(info.TotdMaps).Contains(mapUid);
    }

    [Test]
    public async Task NadeoLiveServices_Leaderboards()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var campaign = await LiveNadeo.GetSeasonalCampaignAsync(account);
        var mapUids = campaign.Playlist.Select(x => x.MapUid).ToList();
        var mapUid = mapUids[0];
        var groupUid = campaign.SeasonUid;

        var mapInfo = await nls.GetMapInfoAsync(mapUid);
        var mapInfos = await nls.GetMapInfosAsync(mapUids);
        var medals = await nls.GetMapMedalRecordsAsync(mapUid, groupUid);
        var top = await nls.GetTopLeaderboardAsync(mapUid, groupUid, length: 5);
        var best = top.Top.Top[0];
        var zoneTop = await nls.GetTopLeaderboardAsync(mapUid, groupUid, best.ZoneId);
        var positions = await nls.GetLeaderboardPositionByTimeAsync(mapUid, groupUid, best.Score.TotalMilliseconds);
        var batchPositions = await nls.GetLeaderboardPositionsByTimeAsync([mapUid, mapUids[1]], [groupUid, groupUid], [best.Score.TotalMilliseconds, best.Score.TotalMilliseconds]);
        var surrounding = await nls.GetSurroundingRecordsAsync(groupUid, mapUid, 1, 1, score: best.Score.TotalMilliseconds, onlyWorld: true);
        var campaignLeaderboard = await nls.GetCampaignLeaderboardAsync(groupUid);
        var seasonRankings = await nls.GetPlayerSeasonRankingsAsync(best.AccountId, groupUid);
        var trophyRankings = await nls.GetPlayerTrophyRankingsAsync(AccountIds);

        await Assert.That(mapInfo?.Uid).IsEqualTo(mapUid);
        await Assert.That(mapInfo?.Name).IsNotNull().And.IsNotEmpty();
        await Assert.That(mapInfos.Select(x => x.Uid)).IsEquivalentTo(mapUids);
        await Assert.That(medals.MapUid).IsEqualTo(mapUid);
        await Assert.That(best.AccountId).IsNotEqualTo(Guid.Empty);
        await Assert.That(best.Score.TotalMilliseconds).IsGreaterThan(0);
        await Assert.That(zoneTop.Tops.SelectMany(x => x.Top)).IsNotEmpty();
        // Not IsNotEmpty: the position endpoints are documented to return [] now and then, even for valid requests.
        await Assert.That(positions).DoesNotContain(x => x.MapUid != mapUid);
        await Assert.That(batchPositions.Select(x => x.MapUid)).DoesNotContain(x => x != mapUid && x != mapUids[1]);
        await Assert.That(surrounding.Tops.SelectMany(x => x.Top)).IsNotEmpty();
        await Assert.That(campaignLeaderboard.Tops).IsNotEmpty();
        await Assert.That(seasonRankings.Zones).IsNotEmpty();
        await Assert.That(trophyRankings.Rankings.Select(x => x.AccountId)).IsEquivalentTo(AccountIds);
    }

    [Test]
    public async Task NadeoLiveServices_Clubs()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var clubs = await nls.GetClubsAsync(1);
        var club = await nls.GetClubAsync(clubs.ClubList[0].Id);
        var members = await nls.GetClubMembersAsync(club.Id, 5);
        var member = members.ClubMemberList[0];
        var memberById = await nls.GetClubMemberAsync(club.Id, member.AccountId);

        await Assert.That(club.Id).IsEqualTo(clubs.ClubList[0].Id);
        await Assert.That(club.Name).IsNotEmpty();
        await Assert.That(members.ClubMemberList).DoesNotContain(x => x.ClubId != club.Id || x.AccountId == Guid.Empty);
        await Assert.That(memberById.AccountId).IsEqualTo(member.AccountId);
    }

    [Test]
    public async Task NadeoLiveServices_ClubMemberRequestsAndRoomPassword()
    {
        // The test accounts aren't members of the first public club with a room.
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var room = (await nls.GetClubRoomsAsync(1)).ClubRoomList[0];
        var memberRequests = await nls.GetClubMemberRequestsAsync(room.ClubId, 5);

        // Readable without a club role, though a non-member sees no requests.
        await Assert.That(memberRequests.ClubMemberList.Count).IsLessThanOrEqualTo(memberRequests.ItemCount);
        await AssertDeniedAsync(() => nls.GetClubRoomPasswordAsync(room.ClubId, room.Id), HttpStatusCode.Forbidden);
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
        await Assert.That(campaign.Campaign.Playlist).IsNotEmpty();
        await Assert.That(activities.ActivityList).IsNotEmpty();

        var groupUid = campaign.Campaign.LeaderboardGroupUid ?? campaign.Campaign.SeasonUid;
        var mapUid = campaign.Campaign.Playlist[0].MapUid;
        var clubCampaignLeaderboard = await nls.GetClubCampaignLeaderboardAsync(groupUid, clubId);
        var clubTop = await nls.GetClubTopLeaderboardAsync(groupUid, mapUid, clubId);
        var ownClubRanking = await nls.GetPlayerClubCampaignRankingAsync(groupUid, clubId);

        await Assert.That(clubCampaignLeaderboard.ClubId).IsEqualTo(clubId);
        await Assert.That(clubCampaignLeaderboard.Top).IsNotEmpty();
        await Assert.That(clubTop.ClubId).IsEqualTo(clubId);
        await Assert.That(clubTop.Top).IsNotEmpty();
        // The account isn't a member, so it has no ranking in the club, which the API answers with zeros, not with an error.
        await Assert.That(ownClubRanking.Position).IsEqualTo(0);

        // These are only for members of the club.
        await AssertDeniedAsync(() => nls.GetClubCampaignsByClubAsync(clubId, 5), HttpStatusCode.Forbidden);
        await AssertDeniedAsync(() => nls.GetPlayerClubMapRecordAsync(groupUid, mapUid, clubId), HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task NadeoLiveServices_OwnClubs()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var myClubs = await nls.GetMyClubsAsync(5);
        var clubPlayerInfo = await nls.GetClubPlayerInfoAsync();

        // A club tag always comes from a club.
        await Assert.That(clubPlayerInfo.TagClubId == 0).IsEqualTo(string.IsNullOrEmpty(clubPlayerInfo.Tag));

        if (account is NadeoAccount.DedicatedServer)
        {
            // A dedicated server account can't join clubs.
            await Assert.That(myClubs.ClubList).IsEmpty();
            return;
        }

        // The service account is in several clubs, some with campaigns.
        await Assert.That(myClubs.ClubList).IsNotEmpty();

        foreach (var club in myClubs.ClubList)
        {
            var campaigns = await nls.GetClubCampaignsByClubAsync(club.Id, 5);

            await Assert.That(campaigns.ClubCampaignList).DoesNotContain(x => x.ClubId != club.Id);
        }
    }

    [Test]
    public async Task NadeoLiveServices_ClubActivities()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var room = (await nls.GetClubRoomsAsync(1)).ClubRoomList[0];
        var roomById = await nls.GetClubRoomAsync(room.ClubId, room.Id);

        var bucket = (await nls.GetClubBucketsAsync(ClubBucketType.MapUpload, 1)).ClubBucketList[0];
        var bucketById = await nls.GetClubBucketAsync(bucket.ClubId, bucket.Id);

        await Assert.That(roomById.Id).IsEqualTo(room.Id);
        await Assert.That(roomById.ClubId).IsEqualTo(room.ClubId);
        await Assert.That(bucketById.Id).IsEqualTo(bucket.Id);
    }

    [Test]
    public async Task NadeoLiveServices_ClubNews()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        // Some of the most popular clubs post news.
        ClubActivity? news = null;
        Club? club = null;

        foreach (var popularClub in (await nls.GetClubsAsync(10)).ClubList)
        {
            news = (await nls.GetClubActivitiesAsync(popularClub.Id, 100)).ActivityList.FirstOrDefault(x => x.ActivityType == "news");

            if (news is not null)
            {
                club = popularClub;
                break;
            }
        }

        if (news is null || club is null)
        {
            throw new InvalidOperationException("None of the most popular clubs has news.");
        }

        var newsById = await nls.GetClubNewsAsync(club.Id, news.ActivityId);

        await Assert.That(newsById.ClubId).IsEqualTo(club.Id);
        await Assert.That(newsById.Headline).IsNotEmpty();
    }

    [Test]
    public async Task NadeoLiveServices_MapReview()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var mapReview = (await nls.GetClubMapReviewRoomsAsync(1)).ClubMapReviewList[0];
        var ownTotdSubmissions = await nls.GetSubmittedMapReviewMapsAsync("totd", 5);
        var waitingTime = await nls.GetMapReviewWaitingTimeAsync("totd");

        await Assert.That(mapReview.ClubId).IsGreaterThan(0);
        await Assert.That(ownTotdSubmissions.SubmittedMaps.Count).IsLessThanOrEqualTo(ownTotdSubmissions.ItemCount);
        await Assert.That(ownTotdSubmissions.SubmittedMaps).DoesNotContain(x => string.IsNullOrEmpty(x.MapUid));
        await Assert.That(waitingTime).IsGreaterThanOrEqualTo(0);

        // A club's map review is only for its members, even when it's listed as public.
        await AssertDeniedAsync(() => nls.GetClubMapReviewActivityAsync(mapReview.ClubId, mapReview.ActivityId), HttpStatusCode.Forbidden);
        await AssertDeniedAsync(() => nls.GetClubMapReviewSubmissionsAsync(mapReview.ClubId, mapReview.ActivityId, 5), HttpStatusCode.Forbidden);
        await AssertDeniedAsync(() => nls.GetClubSubmittedMapReviewMapsAsync(mapReview.ClubId, mapReview.ActivityId, 5), HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task NadeoLiveServices_OwnAccount()
    {
        var nls = await LiveNadeo.GetLiveServicesAsync(account);
        var accountId = GetOwnAccountId(await LiveNadeo.GetServicesAsync(account));

        var campaign = await LiveNadeo.GetSeasonalCampaignAsync(account);

        var serverAccounts = await nls.GetDedicatedServerAccountsAsync();
        var uploadedMaps = await nls.GetUploadedMapsAsync(5);
        var favoriteMaps = await nls.GetFavoriteMapsAsync(5);
        var ownTrophyRanking = await nls.GetOwnTrophyRankingAsync();
        var playerRecords = await nls.GetPlayerRecordsAsync(campaign.Playlist.Select(x => x.MapUid));

        await Assert.That(serverAccounts.PlayerServerAccount.Count).IsLessThanOrEqualTo(serverAccounts.ItemCount);
        await Assert.That(uploadedMaps.MapList).DoesNotContain(x => x.Author != accountId && x.Submitter != accountId);
        await Assert.That(favoriteMaps.MapList.Count).IsLessThanOrEqualTo(5);
        await Assert.That(ownTrophyRanking.AccountId).IsEqualTo(accountId);
        await Assert.That(playerRecords).DoesNotContain(x => string.IsNullOrEmpty(x.MapUid));
    }

    [Test]
    public async Task NadeoMeetServices_Competitions()
    {
        var nms = await LiveNadeo.GetMeetServicesAsync(account);
        var nls = await LiveNadeo.GetLiveServicesAsync(account);

        var competitions = await nms.GetCompetitionsAsync(1);
        var competition = await nms.GetCompetitionAsync(competitions[0].LiveId);

        var clubCompetition = (await nls.GetClubCompetitionsAsync(1)).ClubCompetitionList[0];
        // Takes the ID of the club activity, not of the competition.
        var clubCompetitionById = await nms.GetClubCompetitionAsync(clubCompetition.Id);
        var myClubCompetitions = await nms.GetMyClubCompetitionsAsync(5);

        await Assert.That(competition.LiveId).IsEqualTo(competitions[0].LiveId);
        await Assert.That(competition.Name).IsNotEmpty();
        await Assert.That(clubCompetitionById.ClubCompetition.ActivityId).IsEqualTo(clubCompetition.Id);
        await Assert.That(clubCompetitionById.ClubCompetition.Competition.Id).IsEqualTo(clubCompetition.CompetitionId);
        await Assert.That(myClubCompetitions.ClubCompetitions.Count).IsLessThanOrEqualTo(myClubCompetitions.ClubCompetitionsCount);
    }

    [Test]
    public async Task NadeoMeetServices_CupOfTheDay()
    {
        var nms = await LiveNadeo.GetMeetServicesAsync(account);

        // null between cups, so only that it can be read is checked.
        _ = await nms.GetCurrentCupOfTheDayAsync();
        var cotd = await GetFinishedCupOfTheDayAsync(nms);
        var competitionId = cotd.Competition.LiveId;

        var leaderboard = await nms.GetCompetitionLeaderboardAsync(competitionId, 5);
        var participants = await nms.GetCompetitionParticipantsAsync(competitionId, 5);
        var rounds = await nms.GetCompetitionRoundsAsync(competitionId);
        var teams = await nms.GetCompetitionTeamsAsync(competitionId);
        var playerMatches = await nms.GetCompetitionPlayerMatchesAsync(competitionId, leaderboard[0].Participant);

        await Assert.That(leaderboard).IsNotEmpty();
        await Assert.That(participants).IsNotEmpty();
        await Assert.That(rounds).IsNotEmpty();
        // A cup of the day is played solo, so it has no teams.
        await Assert.That(teams).IsEmpty();
        await Assert.That(playerMatches).IsNotEmpty();

        var match = (await nms.GetCompetitionRoundMatchesAsync(rounds[0].Id.ToString(), 1)).Matches[0];
        var matchResults = await nms.GetCompetitionMatchResultsAsync(match.ClubMatchLiveId, 5);
        var matchInfo = await nms.GetMatchAsync(match.ClubMatchLiveId);
        var matchTeams = await nms.GetMatchTeamsAsync(match.ClubMatchLiveId);
        var matchParticipants = await nms.GetMatchParticipantsAsync(match.ClubMatchLiveId);

        await Assert.That(matchResults.MatchLiveId).IsEqualTo(match.ClubMatchLiveId);
        await Assert.That(matchInfo.LiveId).IsEqualTo(match.ClubMatchLiveId);
        await Assert.That(matchParticipants).IsNotEmpty();
        await Assert.That(matchTeams).IsEmpty();
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

        await Assert.That(players).Count().IsEqualTo(2);
        await Assert.That(ranks.Results.Select(x => x.Player)).IsEquivalentTo(players);
        await Assert.That(progressions.Progressions).IsNotEmpty();
        await Assert.That(divisions.Divisions).IsNotEmpty();
        await Assert.That(status.MatchmakingStatus).IsNotEmpty();
        // null when no Super Royal is scheduled.
        await Assert.That(superRoyalStatus?.Status).IsNotEqualTo(string.Empty);
        // The test accounts don't play Super Royal, so they have no statistics or only zeros.
        await Assert.That(superRoyalStatistics is null
            || superRoyalStatistics.MasterWon + superRoyalStatistics.GoldWon + superRoyalStatistics.SilverWon + superRoyalStatistics.BronzeWon == 0).IsTrue();
    }

    /// <summary>
    /// Checks that the account isn't allowed to send the request: 401 by default, which is what a dedicated server account gets
    /// for a request that only a service account may send.
    /// </summary>
    private static async Task AssertDeniedAsync(Func<Task> request, HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
    {
        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(request);

        await Assert.That(ex!.StatusCode).IsEqualTo(statusCode);
    }

    private static Guid GetOwnAccountId(NadeoServices ns)
    {
        return ns.Handler.JWT?.Subject ?? throw new InvalidOperationException("The token has no account ID.");
    }

    private static async Task<CupOfTheDay> GetFinishedCupOfTheDayAsync(NadeoMeetServices nms)
    {
        // Each cup also has empty per-platform editions (pc, psn, xbl, luna) next to the crossplay one with the players.
        var cups = await nms.GetCupsOfTheDayAsync(CupOfTheDayType.COTD, 30);

        return cups.COTDs.Where(x => x.EndDate < DateTimeOffset.UtcNow && x.Competition.NbPlayers > 0).MaxBy(x => x.EndDate)
            ?? throw new InvalidOperationException("No finished cup of the day found.");
    }
}
