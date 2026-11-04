using System.Text;

namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoAPIAuthorizationTests
{
    private const string AuthenticationUrl = "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic";
    private const string RefreshUrl = "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh";

    private static NadeoAPI CreateService(string name, HttpClient http, NadeoAPIHandler? handler = null) => name switch
    {
        nameof(NadeoServices) => new NadeoServices(http, handler),
        nameof(NadeoLiveServices) => new NadeoLiveServices(http, handler),
        nameof(NadeoMeetServices) => new NadeoMeetServices(http, handler),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    [Test]
    [Arguments(nameof(NadeoServices), "NadeoServices")]
    [Arguments(nameof(NadeoLiveServices), "NadeoLiveServices")]
    [Arguments(nameof(NadeoMeetServices), "NadeoLiveServices")]
    public async Task AuthorizeAsync_SendsBasicCredentialsWithAudience(string service, string expectedAudience)
    {
        var fake = new FakeHttpMessageHandler(request => NadeoTokens.Valid(expectedAudience, out _));
        using var api = CreateService(service, fake.CreateClient());

        await api.AuthorizeAsync("login", "password");

        var request = fake.Request;
        await Assert.That(request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(request.Url).IsEqualTo(AuthenticationUrl);
        await Assert.That(request.Authorization?.Scheme).IsEqualTo("Basic");
        await Assert.That(request.Authorization?.Parameter).IsEqualTo(Convert.ToBase64String(Encoding.UTF8.GetBytes("login:password")));
        await Assert.That(request.Body).IsEqualTo($$"""{"audience":"{{expectedAudience}}"}""");
    }

    [Test]
    public async Task AuthorizeAsync_DecodesTokenTimes()
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);
        var refreshAt = DateTimeOffset.FromUnixTimeSeconds(1_899_998_000);
        var accessToken = NadeoTokens.AccessToken("NadeoServices", expiresAt, refreshAt, account: "my-account");
        var fake = FakeHttpMessageHandler.Json(NadeoTokens.Response(accessToken));
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.AuthorizeAsync("login", "password");

        await Assert.That(ns.ExpirationTime).IsEqualTo(expiresAt);
        await Assert.That(ns.RefreshAt).IsEqualTo(refreshAt);
        await Assert.That(ns.Handler.JWT?.Account).IsEqualTo("my-account");
    }

    [Test]
    [Arguments("", "password")]
    [Arguments("login", "")]
    public async Task AuthorizeAsync_EmptyCredentials_ThrowsWithoutRequest(string login, string password)
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<ArgumentException>(() => ns.AuthorizeAsync(login, password));
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task AuthorizeAsync_FailedAuthentication_Throws()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<HttpRequestException>(() => ns.AuthorizeAsync("login", "wrong"));
        await Assert.That(ns.ExpirationTime).IsNull();
    }

    [Test]
    public async Task Requests_AfterAuthorization_UseNadeoTokenHeader()
    {
        var accessToken = "";
        var fake = FakeHttpMessageHandler.Sequence(
            () => NadeoTokens.Valid("NadeoServices", out accessToken),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.AuthorizeAsync("login", "password");
        await ns.GetZonesAsync();

        var request = fake.Requests[1];
        await Assert.That(request.Url).IsEqualTo("https://prod.trackmania.core.nadeo.online/zones/");
        await Assert.That(request.Authorization?.Scheme).IsEqualTo("nadeo_v1");
        await Assert.That(request.Authorization?.Parameter).IsEqualTo($"t={accessToken}");
    }

    [Test]
    public async Task Requests_WithPendingCredentials_AuthorizeOnlyOnce()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => NadeoTokens.Valid("NadeoServices", out _),
            () => FakeResponse.Json("[]"),
            () => FakeResponse.Json("[]"));
        var handler = new NadeoAPIHandler { PendingCredentials = new NadeoAPICredentials("login", "password") };
        using var ns = new NadeoServices(fake.CreateClient(), handler);

        await ns.GetZonesAsync();
        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo(
        [
            AuthenticationUrl,
            "https://prod.trackmania.core.nadeo.online/zones/",
            "https://prod.trackmania.core.nadeo.online/zones/"
        ]);
        await Assert.That(handler.PendingCredentials).IsNull();
    }

    [Test]
    public async Task Requests_AfterExpiration_RefreshTokenFirst()
    {
        var expiredToken = NadeoTokens.AccessToken("NadeoServices", DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(-30));
        var refreshedToken = "";
        var fake = FakeHttpMessageHandler.Sequence(
            () => FakeResponse.Json(NadeoTokens.Response(expiredToken, refreshToken: "first-refresh-token")),
            () => NadeoTokens.Valid("NadeoServices", out refreshedToken),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.AuthorizeAsync("login", "password");
        await ns.GetZonesAsync();

        var refresh = fake.Requests[1];
        await Assert.That(refresh.Url).IsEqualTo(RefreshUrl);
        await Assert.That(refresh.Authorization?.Parameter).IsEqualTo("t=first-refresh-token");
        await Assert.That(fake.Requests[2].Authorization?.Parameter).IsEqualTo($"t={refreshedToken}");
    }

    [Test]
    public async Task Requests_AfterExpiration_WithoutAutomaticAuthorization_DoNotRefresh()
    {
        var expiredToken = NadeoTokens.AccessToken("NadeoServices", DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(-30));
        var fake = FakeHttpMessageHandler.Sequence(
            () => FakeResponse.Json(NadeoTokens.Response(expiredToken)),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient(), automaticallyAuthorize: false);

        await ns.AuthorizeAsync("login", "password");
        await ns.GetZonesAsync();

        await Assert.That(fake.Requests[1].Url).IsEqualTo("https://prod.trackmania.core.nadeo.online/zones/");
    }

    [Test]
    public async Task RefreshAsync_BeforeRefreshTime_ReturnsFalseWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => NadeoTokens.Valid("NadeoServices", out _));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync("login", "password");

        var refreshed = await ns.RefreshAsync();

        await Assert.That(refreshed).IsFalse();
        await Assert.That(fake.Requests).Count().IsEqualTo(1);
    }

    [Test]
    public async Task RefreshAsync_WithoutAuthorization_ReturnsFalse()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var ns = new NadeoServices(fake.CreateClient());

        var refreshed = await ns.RefreshAsync();

        await Assert.That(refreshed).IsFalse();
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task Constructor_AddsLibraryUserAgentOnce()
    {
        using var http = new HttpClient();

        _ = new NadeoServices(http);
        _ = new NadeoLiveServices(http);

        var products = http.DefaultRequestHeaders.UserAgent.Where(x => x.Product?.Name == "ManiaAPI.NET");
        await Assert.That(products).Count().IsEqualTo(1);
    }

    [Test]
    [Arguments("aa")]
    [Arguments("aaa")]
    [Arguments("aaaa")]
    public async Task JwtPayload_DecodesEveryPaddingLength(string account)
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);
        var token = NadeoTokens.AccessToken("NadeoServices", expiresAt, expiresAt.AddMinutes(-30), account);

        var payload = JwtPayloadNadeoAPI.DecodeFromAccessToken(token);

        await Assert.That(payload.Account).IsEqualTo(account);
        await Assert.That(payload.ExpirationTime).IsEqualTo(expiresAt);
    }
}
