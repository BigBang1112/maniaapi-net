namespace ManiaAPI.ManiaPlanetAPI.Tests.Live;

/// <summary>
/// Live tests with a token from the client credentials flow. The endpoints for a player (<c>/me/...</c>) need a token from the
/// authorization code flow, so here they are checked to be rejected.
/// </summary>
[LiveTest]
[NotInParallel("ManiaPlanet")]
public class ManiaPlanetLiveTests
{
    /// <summary>
    /// A title pack by the library's author, so it won't be taken down.
    /// </summary>
    private const string TitleUid = "Nadeo_Envimix@bigbang1112";

    /// <summary>
    /// Tiger Blood, a well-known TMStadium map.
    /// </summary>
    private const string MapUid = "BSnAjxY3IlQQt4bg1I0PGdgrba0";

    // One token for every test, so the live tests don't request a new one each time.
    private static readonly Lazy<Task<ManiaPlanetAPI>> api = new(AuthorizeAsync);

    private static async Task<ManiaPlanetAPI> AuthorizeAsync()
    {
        var api = new ManiaPlanetAPI();
        await api.AuthorizeAsync(LiveSettings.ManiaPlanet.ClientId, LiveSettings.ManiaPlanet.ClientSecret);
        return api;
    }

    [Test]
    public async Task ManiaPlanetAPI_TitlesZonesAndServers()
    {
        var mp = await api.Value;

        var titles = await mp.SearchTitlesAsync(filters: ["trackmania"], orderBy: "lastUpdate");
        var title = await mp.GetTitleByUidAsync(titles[0].Uid);
        var zones = await mp.GetZonesAsync();
        var servers = await mp.SearchOnlineServersAsync(titleUids: ["TMStadium@nadeo"], zone: "World|Europe");

        await Assert.That(title?.Uid).IsEqualTo(titles[0].Uid);
        await Assert.That(title?.Name).IsNotNull().And.IsNotEmpty();
        await Assert.That(zones).Contains("World");
        // There may be no server online, but the ones that are must be readable.
        await Assert.That(servers).DoesNotContain(x => string.IsNullOrEmpty(x.Login) || x.Title != "TMStadium@nadeo");
    }

    [Test]
    public async Task ManiaPlanetAPI_MapsAndTitleScripts()
    {
        var mp = await api.Value;

        var map = await mp.GetMapByUidAsync(MapUid);
        var scripts = await mp.GetTitleScriptsAsync("TMStadium@nadeo");

        await Assert.That(map?.Uid).IsEqualTo(MapUid);
        await Assert.That(map?.AuthorLogin).IsNotNull().And.IsNotEmpty();
        await Assert.That(scripts).IsNotEmpty();
        await Assert.That(scripts).DoesNotContain(x => string.IsNullOrEmpty(x.FileName));
    }

    [Test]
    [Arguments("me")]
    [Arguments("me/titles/created")]
    [Arguments("me/dedicated")]
    [Arguments("me/maps")]
    [Arguments("me/email")]
    public async Task PlayerEndpoint_WithClientCredentials_IsForbidden(string endpoint)
    {
        var mp = await api.Value;

        Func<Task> request = endpoint switch
        {
            "me" => () => mp.GetPlayerAsync(),
            "me/titles/created" => () => mp.GetTitlesAsync(),
            "me/dedicated" => () => mp.GetDedicatedAccountsAsync(),
            "me/maps" => () => mp.GetMapsAsync(),
            "me/email" => () => mp.GetEmailAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint))
        };

        var ex = await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(request);

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task AuthorizeAsync_UnknownClient_IsUnauthorized()
    {
        using var mp = new ManiaPlanetAPI();

        var ex = await Assert.ThrowsAsync<ManiaPlanetAPIResponseException>(
            () => mp.AuthorizeAsync($"no-such-client-{Guid.NewGuid():N}", Guid.NewGuid().ToString("N")));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(mp.Handler.Credentials).IsNull();
    }

    [Test]
    public async Task ManiaPlanetIngameAPI_Titles()
    {
        var ingame = new ManiaPlanetIngameAPI();

        var titles = await ingame.SearchTitlesAsync(filters: ["trackmania", "solo"], orderBy: "lastUpdate");
        var title = await ingame.GetTitleByUidAsync(TitleUid);
        using var download = await ingame.DownloadTitleAsync(HttpMethod.Head, TitleUid);

        await Assert.That(titles).IsNotEmpty();
        await Assert.That(title?.Uid).IsEqualTo(TitleUid);
        await Assert.That(download.IsSuccessStatusCode).IsTrue();
        await Assert.That(download.Content.Headers.ContentLength ?? 0).IsGreaterThan(0);
    }

    [Test]
    public async Task ManiaPlanetIngameAPI_InvalidToken_IsUnauthorized()
    {
        // The token comes from the game, so a made-up one shows how a rejected token is reported.
        var ingame = new ManiaPlanetIngameAPI();

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => ingame.AuthenticateAsync("bigbang1112", "not-a-real-token"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
