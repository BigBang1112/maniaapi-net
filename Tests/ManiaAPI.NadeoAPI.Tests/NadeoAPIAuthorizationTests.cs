using System.Text;
using System.Text.Json;

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
    public async Task AuthorizeAsync_FailedAuthentication_ThrowsWithErrorDetails()
    {
        var fake = FakeHttpMessageHandler.Json("""{"code":"C-AA-00-03","message":"Invalid credentials."}""", HttpStatusCode.Unauthorized);
        using var ns = new NadeoServices(fake.CreateClient());

        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.AuthorizeAsync(Login, "wrong"));

        await Assert.That(ex!.Message).IsEqualTo("Invalid credentials.");
        await Assert.That(ex.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task AuthorizeAsync_FailedAuthentication_StoresNoToken()
    {
        var fake = FakeHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized);
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.AuthorizeAsync(Login, "wrong"));

        await Assert.That(ns.ExpirationTime).IsNull();
    }

    [Test]
    public async Task AuthorizeAsync_NetworkFailure_ThrowsResponseException()
    {
        var fake = new FakeHttpMessageHandler(_ => throw new HttpRequestException("Connection refused"));
        using var ns = new NadeoServices(fake.CreateClient());

        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.AuthorizeAsync(Login, Password));

        await Assert.That(ex!.InnerException).IsTypeOf<HttpRequestException>();
    }

    [Test]
    [Arguments("null")]
    [Arguments("""{"accessToken":null,"refreshToken":"refresh-token"}""")]
    public async Task AuthorizeAsync_ResponseWithoutTokens_ThrowsJsonException(string json)
    {
        var fake = FakeHttpMessageHandler.Json(json);
        using var ns = new NadeoServices(fake.CreateClient());

        await Assert.ThrowsAsync<JsonException>(() => ns.AuthorizeAsync(Login, Password));
    }

    [Test]
    public async Task RefreshAsync_FailedRefresh_ThrowsResponseException()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            ExpiredTokenResponse,
            () => FakeResponse.Json("""{"message":"Refresh token expired."}""", HttpStatusCode.Unauthorized));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.RefreshAsync().AsTask());

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
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
    [Arguments(HttpStatusCode.Unauthorized)]
    [Arguments(HttpStatusCode.Forbidden)]
    public async Task GetZonesAsync_PendingCredentialsRejected_FailsLaterRequestsWithoutSigningInAgain(HttpStatusCode status)
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => FakeResponse.Json("""{"message":"Invalid credentials."}""", status));
        using var ns = new NadeoServices(fake.CreateClient(), PendingCredentialsHandler());

        var first = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());
        var second = await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl]);
        await Assert.That(second!.StatusCode).IsEqualTo(status);
        await Assert.That(second.Message).IsEqualTo(first!.Message);
        await Assert.That(second.InnerException).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GetZonesAsync_PendingCredentialsFailTemporarily_SignsInAgainOnNextRequest()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => FakeResponse.Json("""{"message":"Service unavailable."}""", HttpStatusCode.ServiceUnavailable),
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient(), PendingCredentialsHandler());

        await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());
        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, AuthenticationUrl, ZonesUrl]);
    }

    [Test]
    public async Task GetZonesAsync_NewPendingCredentialsAfterRejection_SignsInWithThem()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => FakeResponse.Json("""{"message":"Invalid credentials."}""", HttpStatusCode.Unauthorized),
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        var handler = PendingCredentialsHandler();
        using var ns = new NadeoServices(fake.CreateClient(), handler);

        await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());
        handler.PendingCredentials = new NadeoAPICredentials(Login, "new-password");
        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, AuthenticationUrl, ZonesUrl]);
        await Assert.That(fake.Requests[1].Authorization?.Parameter).IsEqualTo(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:new-password")));
    }

    [Test]
    public async Task AuthorizeAsync_AfterPendingCredentialsRejected_RequestsSucceed()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => FakeResponse.Json("""{"message":"Invalid credentials."}""", HttpStatusCode.Unauthorized),
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient(), PendingCredentialsHandler());

        await Assert.ThrowsAsync<NadeoAPIResponseException>(() => ns.GetZonesAsync());
        await ns.AuthorizeAsync(Login, "new-password");
        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, AuthenticationUrl, ZonesUrl]);
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
    public async Task GetZonesAsync_PastRefreshTimeBeforeExpiration_RefreshesFirst()
    {
        var refreshedToken = "";
        var fake = FakeHttpMessageHandler.Sequence(
            DueTokenResponse,
            () => NadeoTokens.Valid(Audience, out refreshedToken),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, RefreshUrl, ZonesUrl]);
        await Assert.That(fake.Requests[2].Authorization?.Parameter).IsEqualTo($"t={refreshedToken}");
    }

    [Test]
    public async Task GetZonesAsync_BeforeRefreshTime_DoesNotRefresh()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => NadeoTokens.Valid(Audience, out _),
            () => FakeResponse.Json("[]"));
        using var ns = new NadeoServices(fake.CreateClient());
        await ns.AuthorizeAsync(Login, Password);

        await ns.GetZonesAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([AuthenticationUrl, ZonesUrl]);
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
    /// A token response past its refresh time that hasn't expired yet, so the next request refreshes it with <see cref="RefreshToken"/>.
    /// </summary>
    private static HttpResponseMessage DueTokenResponse()
    {
        var dueToken = NadeoTokens.AccessToken(Audience, DateTimeOffset.UtcNow.AddMinutes(5), DateTimeOffset.UtcNow.AddMinutes(-1));
        return FakeResponse.Json(NadeoTokens.Response(dueToken, refreshToken: RefreshToken));
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
