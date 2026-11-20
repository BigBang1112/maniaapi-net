namespace ManiaAPI.UnitedLadder.Tests.Live;

[LiveTest]
[NotInParallel(nameof(UnitedLadder))]
public class UnitedLadderLiveTests
{
    private const string Login = "bigbang1112";
    private const string ZonePath = "World|Czech republic";
    private const string TrackUid = "BeySZdnfuSh4nHY5xztiXLmlrXe"; // A01-Race

    private static UnitedLadder CreateClient()
    {
        var http = new HttpClient(new PacedHttpMessageHandler()) { Timeout = TimeSpan.FromSeconds(45) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.UnitedLadder.Tests/1.0 (contact: petrpiv1@gmail.com)");
        return new UnitedLadder(http);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GetPlayerAsync_ReturnsPlayer(bool forceWebServices)
    {
        using var ul = CreateClient();

        var player = await ul.GetPlayerAsync(Login, forceWebServices);

        await Assert.That(player.Login).IsEqualTo(Login);
        await Assert.That(player.Id).IsGreaterThan(0);
        await Assert.That(player.Nickname).IsNotEmpty();
        await Assert.That(player.Path).StartsWith("World");
        await Assert.That(player.IdZone).IsGreaterThan(0);
        await Assert.That(player.Solo).IsNotNull();
        await Assert.That(player.Solo!.Ranks).IsNotEmpty();
        await Assert.That(player.Multiplayer).IsNotNull();
    }

    [Test]
    public async Task GetPlayerAsync_UnknownLogin_IsNotFound()
    {
        using var ul = CreateClient();

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => ul.GetPlayerAsync($"nsl{Guid.NewGuid():N}"[..12]));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [Arguments("World")]
    [Arguments(ZonePath)]
    public async Task GetSoloRankingsAsync_ReturnsRankedPlayers(string zone)
    {
        using var ul = CreateClient();

        var players = await ul.GetSoloRankingsAsync(zone);

        await Assert.That(players).IsNotEmpty();
        await Assert.That(players.Count).IsLessThanOrEqualTo(100);
        await Assert.That(players).DoesNotContain(p => p.Id <= 0 || string.IsNullOrEmpty(p.Login) || p.GlobalRank <= 0 || p.Points < 0);
        await Assert.That(players.Select(p => p.Points)).IsOrderedByDescending(p => p);
    }

    [Test]
    public async Task GetSoloRankingsAsync_NextPageContainsDifferentPlayers()
    {
        using var ul = CreateClient();

        var first = await ul.GetSoloRankingsAsync(page: 0);
        var second = await ul.GetSoloRankingsAsync(page: 1);

        await Assert.That(first).IsNotEmpty();
        await Assert.That(second).IsNotEmpty();
        await Assert.That(second.Select(p => p.Id).Intersect(first.Select(p => p.Id))).IsEmpty();
        await Assert.That(second[0].GlobalRank).IsGreaterThan(first[^1].GlobalRank);
    }

    [Test]
    [Arguments("Merge")]
    [Arguments("Stadium")]
    public async Task GetMultiplayerRankingsAsync_ReturnsRankedPlayers(string environment)
    {
        using var ul = CreateClient();

        var players = await ul.GetMultiplayerRankingsAsync(environment);

        await Assert.That(players).IsNotEmpty();
        await Assert.That(players).DoesNotContain(p => p.Id <= 0 || string.IsNullOrEmpty(p.Login) || p.Points < 0);
        await Assert.That(players.Select(p => p.Points)).IsOrderedByDescending(p => p);
    }

    [Test]
    [Arguments("World")]
    [Arguments(ZonePath)]
    public async Task GetTrackLeaderboardAsync_ReturnsTrackAndRecords(string zone)
    {
        using var ul = CreateClient();

        var track = await ul.GetTrackLeaderboardAsync(TrackUid, zone);

        await Assert.That(track.Uid).IsEqualTo(TrackUid);
        await Assert.That(track.Name).IsEqualTo("A01-Race");
        await Assert.That(track.Envi).IsEqualTo("Stadium");
        await Assert.That(track.Records).IsNotEmpty();
        await Assert.That(track.RecordCount).IsGreaterThanOrEqualTo(track.Records.Count);
        await Assert.That(track.Records).DoesNotContain(r => r.Id <= 0 || string.IsNullOrEmpty(r.Login) || r.Points <= 0 || r.GlobalRank <= 0);
    }

    [Test]
    public async Task GetPlayerMapsAsync_ReturnsOfficialMapResults()
    {
        using var ul = CreateClient();

        var maps = await ul.GetPlayerMapsAsync(Login);

        await Assert.That(maps).IsNotEmpty();
        await Assert.That(maps).DoesNotContain(m => string.IsNullOrEmpty(m.Uid) || string.IsNullOrEmpty(m.Envi) || m.Points < 0);
    }

    [Test]
    public async Task GetPlayerSoloBreakdownAsync_ReturnsEnvironmentAndCampaignStatistics()
    {
        using var ul = CreateClient();

        var breakdown = await ul.GetPlayerSoloBreakdownAsync(Login);

        await Assert.That(breakdown.Environments).IsNotEmpty();
        await Assert.That(breakdown.Campaigns).IsNotEmpty();
        await Assert.That(breakdown.Summary.TotalMaps).IsGreaterThan(0);
        await Assert.That(breakdown.Summary.TotalPoints).IsGreaterThanOrEqualTo(0);
        await Assert.That(breakdown.Environments.Values).DoesNotContain(e => e.MapCount < 0 || e.TopMaps is null || e.AllMaps is null);
        await Assert.That(breakdown.Campaigns.Values).DoesNotContain(c => c.EnvironmentCount < 0 || c.Environments is null);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task CheckLoginAsync_ReportsRegistrationStatus(bool registered)
    {
        using var ul = CreateClient();
        var login = registered ? Login : $"nsl{Guid.NewGuid():N}"[..12];

        var result = await ul.CheckLoginAsync(login);

        await Assert.That(result.Registered).IsEqualTo(registered);
        if (!registered)
        {
            await Assert.That(result.IsUnited).IsFalse();
        }
    }

    [Test]
    public async Task GetPlayerHistoryAsync_RespectsDateRange()
    {
        using var ul = CreateClient();
        var end = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var start = end.AddDays(-7);

        var history = await ul.GetPlayerHistoryAsync(Login, start, end);

        // History can be empty if the service hasn't collected this player during the requested week.
        await Assert.That(history).IsNotNull();
        await Assert.That(history).DoesNotContain(h => h.DataDate < start || h.DataDate > end);
        await Assert.That(history).DoesNotContain(h => h.FullData is null && h.SummaryData is null);
        await Assert.That(history.Where(h => h.FullData is not null)).DoesNotContain(h => h.FullData!.Login != Login);
    }

    [Test]
    [Arguments("skillpoints", null)]
    [Arguments("ladderpoints", "Stadium")]
    [Arguments("nickname", null)]
    public async Task GetPlayerProgressionAsync_ReturnsValuesWithinDateRange(string field, string? environment)
    {
        using var ul = CreateClient();
        var end = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);
        var start = end.AddDays(-7);

        var progression = await ul.GetPlayerProgressionAsync(Login, field, environment, start, end);

        // The selected metric may have no stored changes during the requested week.
        await Assert.That(progression).IsNotNull();
        await Assert.That(progression).DoesNotContain(p => p.Date < start || p.Date > end || string.IsNullOrEmpty(p.Value));
    }

    [Test]
    [Arguments("solo", "solo")]
    [Arguments("multi", "Stadium")]
    [Arguments("track", TrackUid)]
    public async Task GetLeaderboardSnapshotAsync_ReturnsReadablePlayers(string type, string environment)
    {
        using var ul = CreateClient();

        var snapshot = await ul.GetLeaderboardSnapshotAsync(type, environment: environment, page: 0);

        // Snapshots expire after 24 hours, so absence is valid; returned records must still be usable.
        await Assert.That(snapshot).IsNotNull();
        await Assert.That(snapshot).DoesNotContain(s => s.Id <= 0 || s.Type != type || s.Zone != "World" || s.ExpiresAt <= s.CreatedAt);
        foreach (var entry in snapshot)
        {
            await Assert.That(entry.GetRankings()).DoesNotContain(p => p.Id <= 0 || string.IsNullOrEmpty(p.Login));
            if (type == "track")
            {
                await Assert.That(entry.GetTrackLeaderboard()!.Uid).IsEqualTo(TrackUid);
            }
        }
    }

    [Test]
    public async Task GetReplayResponseAsync_DownloadsAdvertisedGhost()
    {
        using var ul = CreateClient();
        var leaderboard = await ul.GetTrackLeaderboardAsync(TrackUid);
        var record = leaderboard.Records.FirstOrDefault(r => !string.IsNullOrEmpty(r.Ghost));
        if (record is null)
        {
            Skip.Test("The current track leaderboard doesn't advertise a downloadable ghost.");
            return;
        }

        using var response = await ul.GetReplayResponseAsync(TrackUid, record.Id);
        var bytes = await response.Content.ReadAsByteArrayAsync();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(bytes.Length).IsGreaterThan(3);
        await Assert.That(System.Text.Encoding.ASCII.GetString(bytes, 0, 3)).IsEqualTo("GBX");
    }

    [Test]
    public async Task GetReplayResponseAsync_UnknownPlayer_IsNotFound()
    {
        using var ul = CreateClient();

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => ul.GetReplayResponseAsync(TrackUid, int.MaxValue));

        await Assert.That(error!.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private sealed class PacedHttpMessageHandler : DelegatingHandler
    {
        public PacedHttpMessageHandler() : base(new HttpClientHandler()) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // The class runs serially; spacing every send also covers tests that make several requests.
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
