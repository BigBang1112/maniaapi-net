namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoLiveAndMeetServicesTests
{
    [Test]
    public async Task NadeoLiveServices_GetTopLeaderboardAsync_UsesLiveServicesAddress()
    {
        var fake = FakeHttpMessageHandler.Json("""{"groupUid":"Personal_Best","mapUid":"UidOfTheMap","tops":[]}""");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        var leaderboard = await nls.GetTopLeaderboardAsync("UidOfTheMap", length: 5, offset: 10);

        await Assert.That(fake.Request.Url).StartsWith(
            "https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/Personal_Best/map/UidOfTheMap/top?length=5&offset=10&onlyWorld=");
        await Assert.That(leaderboard.MapUid).IsEqualTo("UidOfTheMap");
        await Assert.That(leaderboard.Tops).IsEmpty();
    }

    [Test]
    public async Task NadeoLiveServices_GetTopLeaderboardAsync_EmptyUid_ThrowsWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await Assert.ThrowsAsync<ArgumentException>(() => nls.GetTopLeaderboardAsync(""));
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task NadeoMeetServices_GetMatchmakingSummaryAsync_UsesMeetAddress()
    {
        var fake = FakeHttpMessageHandler.Json("""{"ranked3v3Id":2,"royalId":3,"superRoyalId":4}""");
        using var nms = new NadeoMeetServices(fake.CreateClient());

        var summary = await nms.GetMatchmakingSummaryAsync();

        await Assert.That(fake.Request.Url).IsEqualTo("https://meet.trackmania.nadeo.club/api/official/summary");
        await Assert.That(summary).IsEqualTo(new MatchmakingSummary(Ranked3v3Id: 2, RoyalId: 3, SuperRoyalId: 4));
    }

    private const string LiveUrl = "https://live-services.trackmania.nadeo.live/api";

    private const string ClubRoomFromServerJson = """
        {
          "creationTimestamp": 1789164080,
          "clubName": "TMFAST",
          "id": 1166614,
          "activityId": 1166614,
          "campaignId": null,
          "name": "Test Room",
          "clubId": 114118,
          "roomId": null,
          "password": false,
          "room": {
            "id": null,
            "scalable": false,
            "serverInfo": null,
            "scriptSettings": [],
            "playerCount": 0,
            "serverAccountId": "86b1f036-bb94-4561-8f30-be8d44c19ecc",
            "name": "",
            "shufflePlaylist": false,
            "region": null,
            "maps": [],
            "script": "TM_Rounds_Online",
            "maxPlayers": 32
          },
          "mediaUrlPngSmall": "",
          "mediaUrl": "",
          "latestEditorAccountId": "3c1505cd-8a53-4e4e-817c-d808c277e64c",
          "nadeo": false,
          "mediaUrlPngLarge": "",
          "mediaTheme": "",
          "popularityLevel": 0,
          "creatorAccountId": "3c1505cd-8a53-4e4e-817c-d808c277e64c",
          "playerServerLogin": "tmfast_8",
          "mediaUrlDds": "",
          "mediaUrlPngMedium": ""
        }
        """;

    [Test]
    public async Task NadeoLiveServices_GetTopLeaderboardAsync_WithZone_RequestsZoneTop()
    {
        var fake = FakeHttpMessageHandler.Json("""{"groupUid":"Personal_Best","mapUid":"UidOfTheMap","tops":[]}""");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.GetTopLeaderboardAsync("UidOfTheMap", "Personal_Best", zoneId: Guid.Parse("301ff90f-7e13-11e8-8060-e284abfd2bc4"));

        await Assert.That(fake.Request.Url).IsEqualTo(
            $"{LiveUrl}/token/leaderboard/group/Personal_Best/map/UidOfTheMap/top?onlyWorld=false&zoneId=301ff90f-7e13-11e8-8060-e284abfd2bc4");
    }

    [Test]
    public async Task NadeoLiveServices_GetClubActivitiesAsync_SendsFolderId()
    {
        var fake = FakeHttpMessageHandler.Json("""{"activityList":[],"maxPage":0,"itemCount":0}""");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.GetClubActivitiesAsync(clubId: 42, length: 10, offset: 0, active: true, folderId: 7);

        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/42/activity?length=10&offset=0&active=True&folderId=7");
    }

    [Test]
    public async Task NadeoLiveServices_GetClubCampaignsByClubAsync_RequestsCampaignsOfClub()
    {
        var fake = FakeHttpMessageHandler.Json("""{"clubCampaignList":[],"maxPage":67,"itemCount":67}""");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        var campaigns = await nls.GetClubCampaignsByClubAsync(42175, length: 1, offset: 5);

        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/42175/campaign?length=1&offset=5");
        await Assert.That(campaigns.ItemCount).IsEqualTo(67);
    }

    [Test]
    public async Task NadeoLiveServices_CreateClubRoomFromServerAsync_PostsServerLogin()
    {
        var fake = FakeHttpMessageHandler.Json(ClubRoomFromServerJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        var room = await nls.CreateClubRoomFromServerAsync(114118, new ClubRoomFromServerCreation { Name = "Test Room", Login = "tmfast_8", FolderId = 0 });

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/114118/room/create-from-server");
        await Assert.That(fake.Request.Body).IsEqualTo("""{"name":"Test Room","login":"tmfast_8","folderId":0}""");
        await Assert.That(room.ActivityId).IsEqualTo(1166614);
        await Assert.That(room.PlayerServerLogin).IsEqualTo("tmfast_8");
        await Assert.That(room.Room.Script).IsEqualTo("TM_Rounds_Online");
    }

    [Test]
    public async Task NadeoLiveServices_EditClubRoomFromServerAsync_PostsOnlySetFields()
    {
        var fake = FakeHttpMessageHandler.Json(ClubRoomFromServerJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.EditClubRoomFromServerAsync(114118, 1166614, new ClubRoomFromServerEdition { Login = "tmfast_7" });

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/114118/room/1166614/edit-from-server");
        await Assert.That(fake.Request.Body).IsEqualTo("""{"login":"tmfast_7"}""");
    }
}
