namespace ManiaAPI.ManiaPlanetAPI.Tests.Integration;

public class ManiaPlanetIngameAPITests
{
    private readonly ManiaPlanetIngameAPI api;

    public ManiaPlanetIngameAPITests()
    {
        api = new ManiaPlanetIngameAPI();
    }

    [Fact]
    public async Task DownloadTitleAsync()
    {
        using var response = await api.DownloadTitleAsync(HttpMethod.Head, "Nadeo_Envimix@bigbang1112");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetTitleByUidAsync()
    {
        var title = await api.GetTitleByUidAsync("Nadeo_Envimix@bigbang1112");

        Assert.NotNull(title);
    }
}
