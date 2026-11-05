namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoLiveServicesTests
{
    private const string LiveUrl = "https://live-services.trackmania.nadeo.live/api";
    private const string MapUid = "UidOfTheMap";
    private const string PersonalBest = "Personal_Best";
    private const int ClubId = 114118;
    private const int ActivityId = 1166614;
    private const string ServerLogin = "tmfast_8";

    private static readonly Guid ZoneId = Guid.Parse("301ff90f-7e13-11e8-8060-e284abfd2bc4");

    private static readonly string EmptyLeaderboardJson = $$"""{"groupUid":"{{PersonalBest}}","mapUid":"{{MapUid}}","tops":[]}""";

    private static readonly string ClubRoomFromServerJson = $$"""
        {
          "creationTimestamp": 1789164080,
          "clubName": "TMFAST",
          "id": {{ActivityId}},
          "activityId": {{ActivityId}},
          "campaignId": null,
          "name": "Test Room",
          "clubId": {{ClubId}},
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
          "playerServerLogin": "{{ServerLogin}}",
          "mediaUrlDds": "",
          "mediaUrlPngMedium": ""
        }
        """;

    [Test]
    public async Task GetTopLeaderboardAsync_RequestsPersonalBestTopFromLiveServices()
    {
        var fake = FakeHttpMessageHandler.Json(EmptyLeaderboardJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.GetTopLeaderboardAsync(MapUid, length: 5, offset: 10);

        await Assert.That(fake.Request.Url).StartsWith($"{LiveUrl}/token/leaderboard/group/{PersonalBest}/map/{MapUid}/top?length=5&offset=10&onlyWorld=");
    }

    [Test]
    public async Task GetTopLeaderboardAsync_ReturnsLeaderboard()
    {
        var fake = FakeHttpMessageHandler.Json(EmptyLeaderboardJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        var leaderboard = await nls.GetTopLeaderboardAsync(MapUid);

        await Assert.That(leaderboard.MapUid).IsEqualTo(MapUid);
        await Assert.That(leaderboard.Tops).IsEmpty();
    }

    [Test]
    public async Task GetTopLeaderboardAsync_EmptyUid_ThrowsWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await Assert.ThrowsAsync<ArgumentException>(() => nls.GetTopLeaderboardAsync(""));

        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetTopLeaderboardAsync_WithZone_RequestsZoneTop()
    {
        var fake = FakeHttpMessageHandler.Json(EmptyLeaderboardJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.GetTopLeaderboardAsync(MapUid, PersonalBest, zoneId: ZoneId);

        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/leaderboard/group/{PersonalBest}/map/{MapUid}/top?onlyWorld=false&zoneId={ZoneId}");
    }

    [Test]
    public async Task GetClubActivitiesAsync_WithFolder_SendsFolderId()
    {
        var fake = FakeHttpMessageHandler.Json("""{"activityList":[],"maxPage":0,"itemCount":0}""");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.GetClubActivitiesAsync(clubId: 42, length: 10, offset: 0, active: true, folderId: 7);

        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/42/activity?length=10&offset=0&active=True&folderId=7");
    }

    [Test]
    public async Task GetClubCampaignsByClubAsync_RequestsCampaignsOfClub()
    {
        var fake = FakeHttpMessageHandler.Json("""{"clubCampaignList":[],"maxPage":0,"itemCount":0}""");
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.GetClubCampaignsByClubAsync(42175, length: 1, offset: 5);

        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/42175/campaign?length=1&offset=5");
    }

    [Test]
    public async Task CreateClubRoomFromServerAsync_PostsServerLogin()
    {
        var fake = FakeHttpMessageHandler.Json(ClubRoomFromServerJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.CreateClubRoomFromServerAsync(ClubId, new ClubRoomFromServerCreation { Name = "Test Room", Login = ServerLogin, FolderId = 0 });

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/{ClubId}/room/create-from-server");
        await Assert.That(fake.Request.Body).IsEqualTo($$"""{"name":"Test Room","login":"{{ServerLogin}}","folderId":0}""");
    }

    [Test]
    public async Task CreateClubRoomFromServerAsync_ReturnsCreatedRoom()
    {
        var fake = FakeHttpMessageHandler.Json(ClubRoomFromServerJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        var room = await nls.CreateClubRoomFromServerAsync(ClubId, new ClubRoomFromServerCreation { Name = "Test Room", Login = ServerLogin });

        await Assert.That(room.ActivityId).IsEqualTo(ActivityId);
        await Assert.That(room.PlayerServerLogin).IsEqualTo(ServerLogin);
        await Assert.That(room.Room.Script).IsEqualTo("TM_Rounds_Online");
    }

    [Test]
    public async Task EditClubRoomFromServerAsync_PostsOnlySetFields()
    {
        var fake = FakeHttpMessageHandler.Json(ClubRoomFromServerJson);
        using var nls = new NadeoLiveServices(fake.CreateClient());

        await nls.EditClubRoomFromServerAsync(ClubId, ActivityId, new ClubRoomFromServerEdition { Login = "tmfast_7" });

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{LiveUrl}/token/club/{ClubId}/room/{ActivityId}/edit-from-server");
        await Assert.That(fake.Request.Body).IsEqualTo("""{"login":"tmfast_7"}""");
    }
}
