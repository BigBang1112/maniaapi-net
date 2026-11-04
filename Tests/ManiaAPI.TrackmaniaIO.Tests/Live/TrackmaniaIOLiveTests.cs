namespace ManiaAPI.TrackmaniaIO.Tests.Live;

[Category("Live"), LiveTest, Retry(2)]
[NotInParallel(nameof(TrackmaniaIO))]
public class TrackmaniaIOLiveTests
{
    private static TrackmaniaIO CreateClient() => new("ManiaAPI.TrackmaniaIO live tests (Discord=bigbang1112)");

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
}
