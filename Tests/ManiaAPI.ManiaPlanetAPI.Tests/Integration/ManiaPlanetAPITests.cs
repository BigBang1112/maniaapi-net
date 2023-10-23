namespace ManiaAPI.ManiaPlanetAPI.Tests.Integration;

public class ManiaPlanetAPITests
{
    [Fact]
    public async Task GetZoneAsync()
    {
        var api = new ManiaPlanetAPI();
        var zone = await api.AuthorizeAsync()
        Assert.Equal("Europe", zone.Path);
    }
}
