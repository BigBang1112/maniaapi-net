using System.Text;

namespace ManiaAPI.NadeoAPI.Tests;

public class NadeoAPIAuthorizationTests
{
    private const string AuthenticationUrl = "https://prod.trackmania.core.nadeo.online/v2/authentication/token/basic";
    private const string RefreshUrl = "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh";
    private const string ZonesUrl = "https://prod.trackmania.core.nadeo.online/zones/";
    private const string Audience = "NadeoServices";
    private const string Login = "login";
    private const string Password = "password";
    private const string RefreshToken = "first-refresh-token";

    [Test]
    [Arguments(nameof(NadeoServices), "NadeoServices")]
    [Arguments(nameof(NadeoLiveServices), "NadeoLiveServices")]
    [Arguments(nameof(NadeoMeetServices), "NadeoLiveServices")]
    public async Task AuthorizeAsync_SendsBasicCredentialsWithAudience(string service, string expectedAudience)
    {
        var fake = new FakeHttpMessageHandler(request => NadeoTokens.Valid(expectedAudience, out _));
        using var api = CreateService(service, fake.CreateClient());

        await api.AuthorizeAsync(Login, Password);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo(AuthenticationUrl);
        await Assert.That(fake.Request.Authorization?.Scheme).IsEqualTo("Basic");
        await Assert.That(fake.Request.Authorization?.Parameter).IsEqualTo(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}")));
        await Assert.That(fake.Request.Body).IsEqualTo($$"""{"audience":"{{expectedAudience}}"}""");
    }

    [Test]
    public async Task AuthorizeAsync_ValidToken_StoresTokenClaims()
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);
        var refreshAt = DateTimeOffset.FromUnixTimeSeconds(1_899_998_000);
        var accessToken = NadeoTokens.AccessToken(Audience, expiresAt, refreshAt, account: "my-account");
        var fake = FakeHttpMessageHandler.Json(NadeoTokens.Response(accessToken));
        using var ns = new NadeoServices(fake.CreateClient());

        await ns.AuthorizeAsync(Login, Password);

        await Assert.That(ns.ExpirationTime).IsEqualTo(expiresAt);
        await Assert.That(ns.RefreshAt).IsEqualTo(refreshAt);
        await Assert.That(ns.Handler.JWT?.Account).IsEqualTo("my-account");
    }

    [Test]
    [Arguments("", Password)]
    [Arguments(Login, "")]
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

        await Assert.ThrowsAsync<HttpRequestException>(() => ns.AuthorizeAsync(Login, "wrong"));
    }

    [Test]
    public async Task AuthorizeAsync_FailedAuthentication_StoresNoToken()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<HttpRequestException>(() => ns.AuthorizeAsync(Login, "wrong"));

        await Assert.That(ns.ExpirationTime).IsNull();
    }

    [Test]
    public async Task GetZonesAsync_AfterAuthorization_SendsNadeoTokenHeader()
    {
        var accessToken = "";
        var fake = FakeHttpMessageHandler.Sequence(
            () => NadeoTokens.Valid(Audience, out accessToken),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests[1].Authorization?.Scheme).IsEqualTo("nadeo_v1");
        await Assert.That(fake.Requests[1].Authorization?.Parameter).IsEqualTo($"t={accessToken}");
    }

    [Test]
    public async Task GetZonesAsync_WithPendingCredentials_AuthorizesOnlyOnce()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient(), PendingCredentialsHandler());

        await ns.GetZonesAsync();
        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, ZonesUrl, ZonesUrl]);
    }

    [Test]
    public async Task GetZonesAsync_WithPendingCredentials_ClearsPendingCredentials()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        var handler = PendingCredentialsHandler();
        using var ns = new NadeoServices(fake.CreateClient(), handler);

        await ns.GetZonesAsync();

        await Assert.That(handler.PendingCredentials).IsNull();
    }

    [Test]
    public async Task GetZonesAsync_TokenExpired_RefreshesWithRefreshToken()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            ExpiredTokenResponse,
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests[1].Url).IsEqualTo(RefreshUrl);
        await Assert.That(fake.Requests[1].Authorization?.Parameter).IsEqualTo($"t={RefreshToken}");
    }

    [Test]
    public async Task GetZonesAsync_TokenExpired_SendsRefreshedToken()
    {
        var refreshedToken = "";
        var fake = FakeHttpMessageHandler.Sequence(
            ExpiredTokenResponse,
            () => NadeoTokens.Valid(Audience, out refreshedToken),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests[2].Authorization?.Parameter).IsEqualTo($"t={refreshedToken}");
    }

    [Test]
    public async Task GetZonesAsync_TokenExpiredWithoutAutomaticAuthorization_DoesNotRefresh()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            ExpiredTokenResponse,
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient(), automaticallyAuthorize: false);
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests[1].Url).IsEqualTo(ZonesUrl);
    }

    [Test]
    public async Task GetZonesAsync_TokenExpiredOver20HoursAfterAuthorizeAsync_ReauthorizesWithCredentials()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            LongExpiredTokenResponse,
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, AuthenticationUrl, ZonesUrl]);
        await Assert.That(fake.Requests[1].Authorization?.Parameter).IsEqualTo(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}")));
    }

    [Test]
    public async Task GetZonesAsync_TokenExpiredOver20HoursWithoutSavedCredentials_RefreshesWithRefreshToken()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            LongExpiredTokenResponse,
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient(), new NadeoAPIHandler { SaveCredentials = false });
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests[1].Url).IsEqualTo(RefreshUrl);
        await Assert.That(fake.Requests[1].Authorization?.Parameter).IsEqualTo($"t={RefreshToken}");
    }

    [Test]
    public async Task GetZonesAsync_TokenExpiredOver20HoursWithPendingCredentials_ReauthorizesWithCredentials()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            LongExpiredTokenResponse,
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient(), PendingCredentialsHandler());

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, AuthenticationUrl, ZonesUrl]);
    }

    [Test]
    public async Task GetZonesAsync_TwoClientsSharingHandlerWithPendingCredentials_AuthorizesOnce()
    {
        var authorization = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fake = new ConcurrentFakeHttpMessageHandler(async request =>
        {
            if (request.RequestUri!.ToString() == AuthenticationUrl)
            {
                await authorization.Task;
                return NadeoTokens.Valid(Audience, out _);
            }

            return FakeResponse.Json("[]");
        });
        var handler = PendingCredentialsHandler();
        using var first = new NadeoServices(fake.CreateClient(), handler);
        using var second = new NadeoServices(fake.CreateClient(), handler);

        // Both requests start while the first authorization is still waiting for its response
        var requests = Task.WhenAll(first.GetZonesAsync(), second.GetZonesAsync());
        authorization.SetResult();
        await requests;

        await Assert.That(fake.Urls.Count(x => x == AuthenticationUrl)).IsEqualTo(1);
    }

    [Test]
    public async Task RefreshAsync_TwoClientsSharingHandlerAtOnce_RefreshesOnce()
    {
        var refresh = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fake = new ConcurrentFakeHttpMessageHandler(async request =>
        {
            switch (request.RequestUri!.ToString())
            {
                case AuthenticationUrl:
                    return ExpiredTokenResponse();
                case RefreshUrl:
                    await refresh.Task;
                    return NadeoTokens.Valid(Audience, out _);
                default:
                    return FakeResponse.Json("[]");
            }
        });
        var handler = new NadeoAPIHandler();
        using var background = new NadeoServices(fake.CreateClient(), handler);
        using var request = new NadeoServices(fake.CreateClient(), handler);
        await background.AuthorizeAsync(Login, Password);

        // Like the hosting package's background refresh running into a request that also finds the token expired
        var refreshes = Task.WhenAll(background.RefreshAsync().AsTask(), request.GetZonesAsync());
        refresh.SetResult();
        await refreshes;

        await Assert.That(fake.Urls.Count(x => x == RefreshUrl)).IsEqualTo(1);
    }

    [Test]
    public async Task RefreshAsync_BeforeRefreshTime_ReturnsFalseWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => NadeoTokens.Valid(Audience, out _));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        var refreshed = await ns.RefreshAsync();

        await Assert.That(refreshed).IsFalse();
        await Assert.That(fake.Requests).Count().IsEqualTo(1);
    }

    [Test]
    public async Task RefreshAsync_WithoutAuthorization_ReturnsFalseWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var ns = new NadeoServices(fake.CreateClient());

        var refreshed = await ns.RefreshAsync();

        await Assert.That(refreshed).IsFalse();
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task Constructor_SameClientForTwoServices_AddsLibraryUserAgentOnce()
    {
        using var http = new HttpClient();

        _ = new NadeoServices(http);
        _ = new NadeoLiveServices(http);

        var products = http.DefaultRequestHeaders.UserAgent.Where(x => x.Product?.Name == "ManiaAPI.NET");
        await Assert.That(products).Count().IsEqualTo(1);
    }

    [Test]
    public async Task Constructor_AddsLibraryUserAgentWithPackageVersion()
    {
        using var http = new HttpClient();

        _ = new NadeoServices(http);

        var product = http.DefaultRequestHeaders.UserAgent.Single(x => x.Product?.Name == "ManiaAPI.NET").Product!;
        // The assembly version comes from the same <Version> in the project file
        await Assert.That(product.Version).IsEqualTo(typeof(NadeoServices).Assembly.GetName().Version!.ToString(3));
    }

    [Test]
    [Arguments("aa")]
    [Arguments("aaa")]
    [Arguments("aaaa")]
    public async Task DecodeFromAccessToken_AnyBase64Padding_DecodesClaims(string account)
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);
        var token = NadeoTokens.AccessToken(Audience, expiresAt, expiresAt.AddMinutes(-30), account);

        var payload = JwtPayloadNadeoAPI.DecodeFromAccessToken(token);

        await Assert.That(payload.Account).IsEqualTo(account);
    }

    private static NadeoAPI CreateService(string name, HttpClient http) => name switch
    {
        nameof(NadeoServices) => new NadeoServices(http),
        nameof(NadeoLiveServices) => new NadeoLiveServices(http),
        nameof(NadeoMeetServices) => new NadeoMeetServices(http),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private static NadeoAPIHandler PendingCredentialsHandler() => new() { PendingCredentials = new NadeoAPICredentials(Login, Password) };

    /// <summary>
    /// A token response that has already expired, so the next request refreshes it with <see cref="RefreshToken"/>.
    /// </summary>
    private static HttpResponseMessage ExpiredTokenResponse()
    {
        var expiredToken = NadeoTokens.AccessToken(Audience, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(-30));
        return FakeResponse.Json(NadeoTokens.Response(expiredToken, refreshToken: RefreshToken));
    }

    /// <summary>
    /// A token response that expired over 20 hours ago, so its refresh token is likely expired too.
    /// </summary>
    private static HttpResponseMessage LongExpiredTokenResponse()
    {
        var expiredToken = NadeoTokens.AccessToken(Audience, DateTimeOffset.UtcNow.AddHours(-21), DateTimeOffset.UtcNow.AddHours(-21.5));
        return FakeResponse.Json(NadeoTokens.Response(expiredToken, refreshToken: RefreshToken));
    }
}
