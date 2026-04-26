using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ManiaAPI.TMX.Tests;

public class MXTests
{
    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    // Medals don't exist on shootmania, so need to specify fields manually
    public async Task SearchMapsAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var maps = await tmx.SearchMapsAsync(new ());
        using var mapGbxResponse = await tmx.GetMapGbxResponseAsync(maps.Results.First());
        using var mapThumbnailResponse = await tmx.GetMapThumbnailResponseAsync(maps.Results.First());
        Assert.NotEmpty(maps.Results);
        Assert.NotNull(mapGbxResponse);
        Assert.NotNull(mapThumbnailResponse);
    }

    [Theory]
    [InlineData(MxSite.Trackmania, 120839)]
    [InlineData(MxSite.Maniaplanet, 224915)]
    [InlineData(MxSite.Shootmania, 5202)]
    public async Task GetMapImageResponseAsync_Success(MxSite site, long mapId)
    {
        var tmx = new MX(site);
        var image = await tmx.GetMapImageResponseAsync(mapId);
        Assert.NotNull(image);
        var image1 = await tmx.GetMapImageResponseAsync(mapId, 1);
        Assert.NotNull(image1);
    }

    [Theory]
    [InlineData(MxSite.Trackmania, 120839)]
    [InlineData(MxSite.Maniaplanet, 224915)]
    [InlineData(MxSite.Shootmania, 5202)]
    public async Task GetMapScreenResponseAsync_Success(MxSite site, long mapId)
    {
        var tmx = new MX(site);
        var image = await tmx.GetMapScreenResponseAsync(mapId);
        Assert.NotNull(image);
        var image1 = await tmx.GetMapScreenResponseAsync(mapId, 1);
        Assert.NotNull(image1);
    }

    [Theory]
    [InlineData(MxSite.Trackmania, 120839)]
    [InlineData(MxSite.Maniaplanet, 224915)]
    [InlineData(MxSite.Shootmania, 5202)]
    public async Task GetMapThumbnailResponseAsync_Success(MxSite site, long mapId)
    {
        var tmx = new MX(site);
        var image = await tmx.GetMapThumbnailResponseAsync(mapId);
        Assert.NotNull(image);
    }

    [Theory]
    [InlineData(MxSite.Trackmania, 1729)]
    [InlineData(MxSite.Maniaplanet, 1388)]
    [InlineData(MxSite.Shootmania, 10)]
    public async Task GetMappackThumbnailResponseAsync_Success(MxSite site, long mappackId)
    {
        var tmx = new MX(site);
        var image = await tmx.GetMappackThumbnailResponseAsync(mappackId);
        Assert.NotNull(image);
    }

    [Theory]
    [InlineData(MxSite.Trackmania, 120839)]
    [InlineData(MxSite.Maniaplanet, 224915)]
    // no replays for shootmania
    public async Task SearchReplaysAsync_Success(MxSite site, long mapId)
    {
        var tmx = new MX(site);
        var replays = await tmx.SearchReplaysAsync(new MX.SearchReplaysParameters { MapId = mapId });
        Assert.NotEmpty(replays.Results);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task SearchUsersAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var users = await tmx.SearchUsersAsync(new ());
        Assert.NotEmpty(users.Results);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task SearchVideosAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var videos = await tmx.SearchVideosAsync(new ());
        Assert.NotEmpty(videos.Results);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task SearchMappacksAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var mappacks = await tmx.SearchMappacksAsync(new ());
        Assert.NotEmpty(mappacks.Results);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task GetMapSearchOrdersAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var orders = await tmx.GetMapSearchOrdersAsync();
        Assert.NotEmpty(orders);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task GetUserSearchOrdersAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var orders = await tmx.GetUserSearchOrdersAsync();
        Assert.NotEmpty(orders);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task GetMappackSearchOrdersAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var orders = await tmx.GetMappackSearchOrdersAsync();
        Assert.NotEmpty(orders);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task GetTagsAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var tags = await tmx.GetTagsAsync();
        Assert.NotEmpty(tags);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task GetMaptypesAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var maptypes = await tmx.GetMaptypesAsync();
        Assert.NotEmpty(maptypes);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task GetVehiclesAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var vehicles = await tmx.GetVehiclesAsync();
        Assert.NotEmpty(vehicles);
    }

    [Theory]
    [InlineData(MxSite.Trackmania)]
    [InlineData(MxSite.Maniaplanet)]
    [InlineData(MxSite.Shootmania)]
    public async Task GetTitlepacksAsync_Success(MxSite site)
    {
        var tmx = new MX(site);
        var titlepacks = await tmx.GetTitlepacksAsync();
        Assert.NotEmpty(titlepacks);
    }
}