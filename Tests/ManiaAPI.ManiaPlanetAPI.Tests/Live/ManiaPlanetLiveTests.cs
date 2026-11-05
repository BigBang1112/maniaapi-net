namespace ManiaAPI.ManiaPlanetAPI.Tests.Live;

[Category("Live"), LiveTest, Retry(2)]
[NotInParallel("ManiaPlanet")]
public class ManiaPlanetLiveTests
{
    private const string TitleUid = "Nadeo_Envimix@bigbang1112";
    private const string MapUid = "BSnAjxY3IlQQt4bg1I0PGdgrba0"; // Tiger Blood

    private static async Task<ManiaPlanetAPI> AuthorizeAsync()
    {
        var clientId = LiveSecrets.Require("ClientId", "MANIAPLANET_API_CLIENTID");
        var clientSecret = LiveSecrets.Require("ClientSecret", "MANIAPLANET_API_CLIENTSECRET");

        var api = new ManiaPlanetAPI();
        await api.AuthorizeAsync(clientId, clientSecret);
        return api;
    }

    [Test]
    public async Task ManiaPlanetAPI_TitlesZonesAndServers()
    {
        using var api = await AuthorizeAsync();

        var titles = await api.SearchTitlesAsync(filters: ["trackmania"], orderBy: "lastUpdate");
        var title = await api.GetTitleByUidAsync(titles[0].Uid);
        var zones = await api.GetZonesAsync();
        _ = await api.SearchOnlineServersAsync(titleUids: ["TMStadium@nadeo"], zone: "World|Europe");

        await Assert.That(title?.Uid).IsEqualTo(titles[0].Uid);
        await Assert.That(zones).Contains("World");
    }

    [Test]
    public async Task ManiaPlanetAPI_UserEndpoints_RejectClientCredentials()
    {
        using var api = await AuthorizeAsync();

        await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetPlayerAsync());
    }

    [Test]
    public async Task ManiaPlanetIngameAPI_Titles()
    {
        var api = new ManiaPlanetIngameAPI();

        var titles = await api.SearchTitlesAsync(filters: ["trackmania", "solo"], orderBy: "lastUpdate");
        var title = await api.GetTitleByUidAsync(TitleUid);
        using var download = await api.DownloadTitleAsync(HttpMethod.Head, TitleUid);

        await Assert.That(titles).IsNotEmpty();
        await Assert.That(title?.Uid).IsEqualTo(TitleUid);
        await Assert.That(download.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task ManiaPlanetAPI_MapsAndTitleScripts()
    {
        using var api = await AuthorizeAsync();

        var map = await api.GetMapByUidAsync(MapUid);
        var scripts = await api.GetTitleScriptsAsync("TMStadium@nadeo");

        await Assert.That(map?.Uid).IsEqualTo(MapUid);
        await Assert.That(scripts).IsNotEmpty();
    }

    [Test]
    public async Task ManiaPlanetAPI_OtherUserEndpoints_RejectClientCredentials()
    {
        using var api = await AuthorizeAsync();

        await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetTitlesAsync());
        await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetDedicatedAccountsAsync());
        await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetMapsAsync());
        await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(() => api.GetEmailAsync());
    }
}
