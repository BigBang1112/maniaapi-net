namespace ManiaAPI.TrackmaniaIO.Tests.Live;

[Category("Live"), LiveTest, Retry(2)]
[NotInParallel(nameof(TrackmaniaIO))]
public class TrackmaniaIOLiveTests
{
    private static TrackmaniaIO CreateClient() => new(new HttpClient(new RateLimitRetryHandler()), "ManiaAPI.TrackmaniaIO live tests (Discord=bigbang1112)");

    /// <summary>
    /// Waits out 429 responses. The library only tracks the rate limit within its own process,
    /// but <c>dotnet test</c> runs each target framework in a separate process at the same time.
    /// </summary>
    private sealed class RateLimitRetryHandler() : DelegatingHandler(new HttpClientHandler())
    {
        private const int MaxAttempts = 6;
        private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(60);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode != HttpStatusCode.TooManyRequests || attempt == MaxAttempts)
                {
                    return response;
                }

                var delay = GetDelay(response);
                response.Dispose();
                await Task.Delay(delay, cancellationToken);
            }
        }

        private static TimeSpan GetDelay(HttpResponseMessage response)
        {
            var delay = response.Headers.RetryAfter?.Delta
                ?? (response.Headers.TryGetValues("X-Ratelimit-Reset", out var values) && int.TryParse(values.FirstOrDefault(), out var seconds)
                    ? TimeSpan.FromSeconds(seconds + 1)
                    : TimeSpan.FromSeconds(10));

            return delay < MaxDelay ? delay : MaxDelay;
        }
    }

    [Test]
    public async Task SeasonalCampaign_ResolvesMapsAndLeaderboards()
    {
        using var tmio = CreateClient();

        var campaigns = await tmio.GetSeasonalCampaignsAsync();
        await Assert.That(campaigns.Campaigns).IsNotEmpty();

        var campaign = await tmio.GetSeasonalCampaignAsync(campaigns.Campaigns[0].Id);
        await Assert.That(campaign.Playlist).IsNotEmpty();

        var mapUid = campaign.Playlist[0].MapUid;
        var map = await tmio.GetMapInfoAsync(mapUid);
        var leaderboard = await tmio.GetLeaderboardAsync(campaign.LeaderboardUid, mapUid);

        await Assert.That(map.MapUid).IsEqualTo(mapUid);
        await Assert.That(leaderboard.Tops).IsNotEmpty();
    }

    [Test]
    public async Task Clubs_ResolveClubDetails()
    {
        using var tmio = CreateClient();

        var clubs = await tmio.GetClubsAsync();
        await Assert.That(clubs.Clubs).IsNotEmpty();

        var club = await tmio.GetClubAsync(clubs.Clubs[0].Id);
        var members = await tmio.GetClubMembersAsync(club.Id);

        await Assert.That(club.Id).IsEqualTo(clubs.Clubs[0].Id);
        await Assert.That(members.Members).IsNotEmpty();
    }

    [Test]
    public async Task GetTrackOfTheDaysAsync_ReturnsDays()
    {
        using var tmio = CreateClient();

        var totds = await tmio.GetTrackOfTheDaysAsync();

        await Assert.That(totds.Days).IsNotEmpty();
    }

    [Test]
    public async Task Competitions_ResolveCompetitionDetails()
    {
        using var tmio = CreateClient();

        var competitions = await tmio.GetCompetitionsAsync();
        await Assert.That(competitions.Competitions).IsNotEmpty();

        var competition = await tmio.GetCompetitionAsync(competitions.Competitions[0].CompetitionId);

        await Assert.That(competition.Id).IsEqualTo(competitions.Competitions[0].CompetitionId);
    }

    [Test]
    public async Task WeeklyCampaigns_ResolveCampaignDetails()
    {
        using var tmio = CreateClient();

        var weekly = await tmio.GetWeeklyCampaignsAsync();
        var weeklyShort = await tmio.GetWeeklyShortCampaignsAsync();
        var weeklyGrand = await tmio.GetWeeklyGrandCampaignsAsync();

        await Assert.That(weekly.Campaigns).IsNotEmpty();
        await Assert.That(weeklyShort.Campaigns).IsNotEmpty();
        await Assert.That(weeklyGrand.Campaigns).IsNotEmpty();

        var weeklyCampaign = await tmio.GetWeeklyCampaignAsync(weekly.Campaigns[0].Id);
        var weeklyShortCampaign = await tmio.GetWeeklyShortCampaignAsync(weeklyShort.Campaigns[0].Id);
        var weeklyGrandCampaign = await tmio.GetWeeklyGrandCampaignAsync(weeklyGrand.Campaigns[0].Id);

        await Assert.That(weeklyCampaign.Id).IsEqualTo(weekly.Campaigns[0].Id);
        await Assert.That(weeklyShortCampaign.Id).IsEqualTo(weeklyShort.Campaigns[0].Id);
        await Assert.That(weeklyGrandCampaign.Id).IsEqualTo(weeklyGrand.Campaigns[0].Id);
    }

    [Test]
    public async Task ClubCampaigns_ResolveCampaignDetails()
    {
        using var tmio = CreateClient();

        var campaigns = await tmio.GetClubCampaignsAsync();
        var item = campaigns.Campaigns.First(x => x.ClubId is not null);

        var campaign = await tmio.GetClubCampaignAsync(item.ClubId!.Value, item.Id);
        var searched = await tmio.GetClubCampaignsAsync(0, item.Name);

        await Assert.That(campaign.Id).IsEqualTo(item.Id);
        await Assert.That(searched.Campaigns.Select(x => x.Id)).Contains(item.Id);
    }

    [Test]
    public async Task Leaderboards_ResolveByMapAndWorldRecords()
    {
        using var tmio = CreateClient();

        var campaigns = await tmio.GetSeasonalCampaignsAsync();
        var campaign = await tmio.GetSeasonalCampaignAsync(campaigns.Campaigns[0].Id);
        var mapUid = campaign.Playlist[0].MapUid;

        var leaderboard = await tmio.GetLeaderboardAsync(mapUid, offset: 0, length: 5);
        var worldRecords = await tmio.GetRecentWorldRecordsAsync(campaign.LeaderboardUid);

        await Assert.That(leaderboard.Tops).IsNotEmpty();
        await Assert.That(worldRecords).IsNotNull();
    }

    [Test]
    public async Task ClubActivitiesAndRooms_ResolveDetails()
    {
        using var tmio = CreateClient();

        var clubs = await tmio.GetClubsAsync();
        var activities = await tmio.GetClubActivitiesAsync(clubs.Clubs[0].Id);
        var searchedClubs = await tmio.GetClubsAsync(0, clubs.Clubs[0].Name);

        await Assert.That(activities.Activities).IsNotEmpty();
        await Assert.That(searchedClubs.Clubs.Select(x => x.Id)).Contains(clubs.Clubs[0].Id);

        var rooms = await tmio.GetClubRoomsAsync();
        var room = await tmio.GetClubRoomAsync(rooms.Rooms[0].ClubId, rooms.Rooms[0].Id);

        await Assert.That(room.Id).IsEqualTo(rooms.Rooms[0].Id);
    }

    [Test]
    public async Task Competitions_Search()
    {
        using var tmio = CreateClient();

        var competitions = await tmio.GetCompetitionsAsync();
        var searched = await tmio.GetCompetitionsAsync(0, competitions.Competitions[0].Name);

        await Assert.That(searched.Competitions.Select(x => x.CompetitionId)).Contains(competitions.Competitions[0].CompetitionId);
    }

    [Test]
    public async Task GetAdsAsync_ReturnsAds()
    {
        using var tmio = CreateClient();

        var ads = await tmio.GetAdsAsync();

        await Assert.That(ads.Ads).IsNotNull();
    }
}
