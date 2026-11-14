namespace ManiaAPI.TrackmaniaWS.Tests.Live;

[LiveTest]
public class TrackmaniaWSLiveTests
{
    private const string Login = "bigbang1112";
    private const string ZonePath = "World|Czech republic";
    private const int ZoneId = 310;
    private const string ChallengeUid = "BeySZdnfuSh4nHY5xztiXLmlrXe"; // A01-Race

    private static TrackmaniaWS CreateWs() => new(LiveSettings.TrackmaniaWS.Username, LiveSettings.TrackmaniaWS.Password);

    [Test]
    public async Task GetPlayerAsync_ReturnsPlayer()
    {
        using var ws = CreateWs();

        var player = await ws.GetPlayerAsync(Login);

        await Assert.That(player.Login).IsEqualTo(Login);
        await Assert.That(player.Id).IsGreaterThan(0);
        await Assert.That(player.Nickname).IsNotEmpty();
        await Assert.That(player.Path).StartsWith("World");
        await Assert.That(player.IdZone).IsGreaterThan(0);
    }

    [Test]
    public async Task GetPlayerAsync_UnknownPlayer_IsNotFound()
    {
        using var ws = CreateWs();

        var ex = await Assert.ThrowsAsync<TrackmaniaWSResponseException>(() => ws.GetPlayerAsync("nosuchplayer98765x"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        await Assert.That(ex.Message).IsNotEmpty();
    }

    [Test]
    public async Task GetPlayerNicknameHtmlAsync_ReturnsHtml()
    {
        using var ws = CreateWs();

        var html = await ws.GetPlayerNicknameHtmlAsync(Login);

        await Assert.That(html).Contains("<span");
    }

    [Test]
    public async Task GetPlayerTagsAsync_ReturnsTags()
    {
        using var ws = CreateWs();

        var tags = await ws.GetPlayerTagsAsync(Login);

        await Assert.That(tags).IsNotEmpty();
        await Assert.That(tags[0].Url).StartsWith("http");
    }

    [Test]
    [Arguments(null, "Merge")]
    [Arguments("Stadium", "Stadium")]
    public async Task GetPlayerMultiplayerRankingAsync_ReturnsRanking(string? environment, string expectedEnvironment)
    {
        using var ws = CreateWs();

        var ranking = await ws.GetPlayerMultiplayerRankingAsync(Login, environment);

        await Assert.That(ranking.Environment).IsEqualTo(expectedEnvironment);
        await Assert.That(ranking.Points).IsGreaterThan(0);
        await Assert.That(ranking.Ranks).IsNotEmpty();
        await Assert.That(ranking.Ranks[0].Path).IsEqualTo("World");
        await Assert.That(ranking.Ranks[0].Rank).IsGreaterThan(0);
    }

    [Test]
    public async Task GetPlayerMultiplayerRankingAsync_InvalidEnvironment_HasServerMessage()
    {
        using var ws = CreateWs();

        var ex = await Assert.ThrowsAsync<TrackmaniaWSResponseException>(() => ws.GetPlayerMultiplayerRankingAsync(Login, "Foo"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(ex.Message).Contains("Foo");
    }

    [Test]
    public async Task GetPlayerSoloRankingAsync_ReturnsRanking()
    {
        using var ws = CreateWs();

        var ranking = await ws.GetPlayerSoloRankingAsync(Login);

        await Assert.That(ranking.Unit).IsEqualTo("Skill Points");
        await Assert.That(ranking.Ranks[0].IdZone).IsEqualTo(1);
    }

    [Test]
    public async Task GetZoneAsync_ReturnsZone()
    {
        using var ws = CreateWs();

        var zone = await ws.GetZoneAsync(ZoneId);

        await Assert.That(zone.Path).IsEqualTo(ZonePath);
    }

    [Test]
    public async Task GetZoneByPathAsync_ReturnsZone()
    {
        using var ws = CreateWs();

        var zone = await ws.GetZoneByPathAsync(ZonePath);

        await Assert.That(zone.Id).IsEqualTo(ZoneId);
    }

    [Test]
    public async Task GetZoneIdAsync_ReturnsId()
    {
        using var ws = CreateWs();

        await Assert.That(await ws.GetZoneIdAsync(ZonePath)).IsEqualTo(ZoneId);
    }

    [Test]
    public async Task GetZonesAsync_ReturnsZones()
    {
        using var ws = CreateWs();

        var zones = await ws.GetZonesAsync(length: 3);

        await Assert.That(zones.Count).IsEqualTo(3);
        await Assert.That(zones[0].Path).IsEqualTo("World");
        await Assert.That(zones[1].PathIds).IsEquivalentTo([1]);
    }

    [Test]
    public async Task GetZoneChildrenAsync_ReturnsChildren()
    {
        using var ws = CreateWs();

        var zones = await ws.GetZoneChildrenAsync(ZoneId, length: 3, sort: ZoneSort.Name, descending: true);

        await Assert.That(zones.Count).IsEqualTo(3);
        await Assert.That(zones.All(x => x.Path.StartsWith(ZonePath + "|"))).IsTrue();
        await Assert.That(zones.Select(x => x.Name)).IsOrderedByDescending(x => x, StringComparer.Ordinal);
    }

    [Test]
    public async Task GetZoneChildrenByPathAsync_ReturnsChildren()
    {
        using var ws = CreateWs();

        var zones = await ws.GetZoneChildrenByPathAsync("World", length: 3);

        await Assert.That(zones.Count).IsEqualTo(3);
        await Assert.That(zones.All(x => x.PathIds is [1])).IsTrue();
    }

    [Test]
    [Arguments(null, "World")]
    [Arguments(ZonePath, ZonePath)]
    public async Task GetSoloLeaderboardAsync_ReturnsPlayers(string? path, string expectedPath)
    {
        using var ws = CreateWs();

        var leaderboard = await ws.GetSoloLeaderboardAsync(path, length: 2);

        await Assert.That(leaderboard.Path).IsEqualTo(expectedPath);
        await Assert.That(leaderboard.Players).IsNotEmpty();
        await Assert.That(leaderboard.Players[0].Player.Login).IsNotEmpty();
    }

    [Test]
    [Arguments(null, "World")]
    [Arguments(ZonePath, ZonePath)]
    public async Task GetSoloChallengeLeaderboardAsync_ReturnsPlayers(string? path, string expectedPath)
    {
        using var ws = CreateWs();

        var leaderboard = await ws.GetSoloChallengeLeaderboardAsync(ChallengeUid, path, length: 2);

        await Assert.That(leaderboard.ChallengeUid).IsEqualTo(ChallengeUid);
        await Assert.That(leaderboard.Path).IsEqualTo(expectedPath);
        await Assert.That(leaderboard.Players.Count).IsEqualTo(2);
        await Assert.That(leaderboard.Players[0].Points).IsGreaterThan(0);
    }

    [Test]
    public async Task GetMultiplayerLeaderboardAsync_ReturnsPlayers()
    {
        using var ws = CreateWs();

        var leaderboard = await ws.GetMultiplayerLeaderboardAsync(ZonePath, "Stadium", length: 2);

        await Assert.That(leaderboard.IdZone).IsEqualTo(ZoneId);
        await Assert.That(leaderboard.Environment).IsEqualTo("Stadium");
        await Assert.That(leaderboard.Players.Count).IsEqualTo(2);
        await Assert.That(leaderboard.Players[0].Rank).IsEqualTo(1);
    }

    [Test]
    public async Task GetMultiplayerZoneLeaderboardAsync_ReturnsZones()
    {
        using var ws = CreateWs();

        var leaderboard = await ws.GetMultiplayerZoneLeaderboardAsync(length: 2);

        await Assert.That(leaderboard.Path).IsEqualTo("World");
        await Assert.That(leaderboard.Zones.Count).IsEqualTo(2);
        await Assert.That(leaderboard.Zones[0].Zone.Path).StartsWith("World|");
    }

    [Test]
    public async Task GetRegisteredPlayerCountAsync_ReturnsCount()
    {
        using var ws = CreateWs();

        await Assert.That(await ws.GetRegisteredPlayerCountAsync()).IsGreaterThan(1_000_000);
    }

    [Test]
    public async Task GetManialinkAsync_ReturnsManialink()
    {
        using var ws = CreateWs();

        var manialink = await ws.GetManialinkAsync("tmx");

        await Assert.That(manialink.Code).IsEqualTo("TMX");
        await Assert.That(manialink.Url).StartsWith("http");
        await Assert.That(manialink.Login).IsNotEmpty();
    }

    [Test]
    public async Task PingAsync_ServiceIsUp()
    {
        // The test route needs no credentials, so any are fine
        using var ws = new TrackmaniaWS("", "");

        await Assert.That(await ws.PingAsync()).IsTrue();
    }

    [Test]
    public async Task GetPlayerAsync_WrongCredentials_IsUnauthorized()
    {
        using var ws = new TrackmaniaWS($"nosuchuser{Guid.NewGuid():N}"[..20], Guid.NewGuid().ToString("N"));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => ws.GetPlayerAsync("bigbang1112"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
