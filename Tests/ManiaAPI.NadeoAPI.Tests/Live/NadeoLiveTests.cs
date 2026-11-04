namespace ManiaAPI.NadeoAPI.Tests.Live;

[Category("Live"), LiveTest, Retry(2)]
[NotInParallel("Nadeo")]
public class NadeoLiveTests
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
        var ns = await LiveNadeo.GetServicesAsync();

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
        var ns = await LiveNadeo.GetServicesAsync();

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
        var nls = await LiveNadeo.GetLiveServicesAsync();

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
        var nls = await LiveNadeo.GetLiveServicesAsync();

        var clubs = await nls.GetClubsAsync(1);
        var club = await nls.GetClubAsync(clubs.ClubList[0].Id);
        var members = await nls.GetClubMembersAsync(club.Id, 5);

        await Assert.That(club.Id).IsEqualTo(clubs.ClubList[0].Id);
        await Assert.That(members.ClubMemberList).IsNotEmpty();
    }

    [Test]
    public async Task NadeoMeetServices_CompetitionsAndMatchmaking()
    {
        var nms = await LiveNadeo.GetMeetServicesAsync();

        var competitions = await nms.GetCompetitionsAsync(1);
        var competition = await nms.GetCompetitionAsync(competitions[0].LiveId);
        var summary = await nms.GetMatchmakingSummaryAsync();
        var rankings = await nms.GetMatchmakingRankingsAsync(summary.Ranked3v3Id, 5);

        await Assert.That(competition.LiveId).IsEqualTo(competitions[0].LiveId);
        await Assert.That(rankings.Results).IsNotEmpty();
    }
}
