using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.TMX.Tests;

public class TMXTests
{
    [Theory]
    [InlineData(TmxSite.TMUF, 754327)]
    [InlineData(TmxSite.TMNF, 2233)]
    public async Task GetReplaysAsync_Success(TmxSite site, long trackId)
    {
        var tmx = new TMX(site);
        var replays = await tmx.GetReplaysAsync(new() { TrackId = trackId });
        using var replayGbxResponse = await tmx.GetReplayGbxResponseAsync(replays.Results.First());
        Assert.NotEmpty(replays.Results);
        Assert.NotNull(replayGbxResponse);
    }

    [Fact]
    public async Task SearchTracksAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var tracks = await tmx.SearchTracksAsync(new());
        using var trackGbxResponse = await tmx.GetTrackGbxResponseAsync(tracks.Results.First());
        using var trackThumbnailResponse = await tmx.GetTrackThumbnailResponseAsync(tracks.Results.First());
        Assert.NotEmpty(tracks.Results);
        Assert.NotNull(trackGbxResponse);
        Assert.NotNull(trackThumbnailResponse);
    }

    [Fact]
    public async Task SearchLeaderboardsAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var lb = await tmx.SearchLeaderboardsAsync(new());
        Assert.NotEmpty(lb.Results);
    }

    [Fact]
    public async Task SearchTrackpacksAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var trackpacks = await tmx.SearchTrackpacksAsync(new());
        Assert.NotEmpty(trackpacks.Results);
    }

    [Fact]
    public async Task SearchUsersAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var users = await tmx.SearchUsersAsync(new());
        Assert.NotEmpty(users.Results);
    }

    [Fact]
    public async Task GetRandomTrackIdAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var randomTrackId = await tmx.GetRandomTrackIdAsync(new());
        Assert.NotNull(randomTrackId);
    }

    [Fact]
    public async Task GetRandomTrackAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var randomTrack = await tmx.GetRandomTrackAsync(new());
        Assert.NotNull(randomTrack);
    }

    [Fact]
    public async Task GetRandomTrackpackIdAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var randomTrackpackId = await tmx.GetRandomTrackpackIdAsync(new());
        Assert.NotNull(randomTrackpackId);
    }

    [Fact]
    public async Task GetRandomTrackpackAsync_Success()
    {
        var tmx = new TMX(TmxSite.TMNF);
        var randomTrackpack = await tmx.GetRandomTrackpackAsync(new());
        Assert.NotNull(randomTrackpack);
    }
}
