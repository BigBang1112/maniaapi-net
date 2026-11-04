using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaAPI.Tests;

public class TrackmaniaAPITests
{
    private const string BaseUrl = "https://api.trackmania.com/api";

    private static readonly Guid AccountA = Guid.Parse("6a43df20-cd1a-4b3b-87b9-a6835a9b416d");
    private static readonly Guid AccountB = Guid.Parse("faedcf21-d61a-4305-9ffe-680b2ee5d65e");

    private static string AccessToken(DateTimeOffset expiresAt) => TestJwt.Create($$"""
        {
          "jti": "token-id",
          "aud": "client-id",
          "iat": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
          "nbf": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
          "exp": {{expiresAt.ToUnixTimeSeconds()}},
          "sub": "client-id",
          "scopes": ["read_favorite"]
        }
        """);

    private static HttpResponseMessage TokenResponse(DateTimeOffset expiresAt, out string accessToken)
    {
        accessToken = AccessToken(expiresAt);
        return FakeResponse.Json($$"""{"token_type":"Bearer","expires_in":3600,"access_token":"{{accessToken}}"}""");
    }

    private static HttpResponseMessage ValidTokenResponse() => TokenResponse(DateTimeOffset.UtcNow.AddHours(1), out _);

    [Test]
    public async Task AuthorizeAsync_PostsClientCredentials()
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(expiresAt, out _));
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());

        await api.AuthorizeAsync("client-id", "client-secret", ["read_favorite"]);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/access_token");
        await Assert.That(fake.Request.Headers["Content-Type"]).StartsWith("application/x-www-form-urlencoded");
        await Assert.That(fake.Request.Body).IsEqualTo("grant_type=client_credentials&client_id=client-id&client_secret=client-secret&scope=read_favorite");
        await Assert.That(api.ExpirationTime).IsEqualTo(expiresAt);
        await Assert.That(api.Handler.Payload!.Scopes).IsEquivalentTo(["read_favorite"]);
        await Assert.That(api.Handler.Credentials?.ClientId).IsEqualTo("client-id");
    }

    [Test]
    public async Task AuthorizeAsync_InvalidClient_ThrowsWithErrorDescription()
    {
        var fake = FakeHttpMessageHandler.Json(
            """{"error":"invalid_client","message":"Client authentication failed","error_description":"Client authentication failed"}""",
            HttpStatusCode.Unauthorized);
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => api.AuthorizeAsync("client-id", "wrong"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(ex.Message).IsEqualTo("Client authentication failed");
        await Assert.That(ex.Response?.Error).IsEqualTo("invalid_client");
        await Assert.That(api.Handler.Credentials).IsNull();
    }

    [Test]
    public async Task GetDisplayNamesAsync_SendsIndexedAccountIdsWithBearerToken()
    {
        var accessToken = "";
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddHours(1), out accessToken),
            () => FakeResponse.Json($$"""{"{{AccountA}}":"BigBang1112","{{AccountB}}":"Player"}"""));
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());
        await api.AuthorizeAsync("client-id", "client-secret");

        var names = await api.GetDisplayNamesAsync([AccountA, AccountB], CancellationToken.None);

        var request = fake.Requests[1];
        await Assert.That(request.Url).IsEqualTo($"{BaseUrl}/display-names?accountId[0]={AccountA}&accountId[1]={AccountB}");
        await Assert.That(request.Authorization?.Scheme).IsEqualTo("Bearer");
        await Assert.That(request.Authorization?.Parameter).IsEqualTo(accessToken);
        await Assert.That(names[AccountA]).IsEqualTo("BigBang1112");
        await Assert.That(names[AccountB]).IsEqualTo("Player");
    }

    [Test]
    public async Task GetAccountIdsAsync_SendsIndexedDisplayNames()
    {
        var fake = FakeHttpMessageHandler.Json($$"""{"BigBang1112":"{{AccountA}}"}""");
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());

        var ids = await api.GetAccountIdsAsync(["BigBang1112"], CancellationToken.None);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/display-names/account-ids?displayName[0]=BigBang1112");
        await Assert.That(ids["BigBang1112"]).IsEqualTo(AccountA);
    }

    [Test]
    public async Task EmptyInputs_ReturnEmptyResultsWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());

        var names = await api.GetDisplayNamesAsync([], CancellationToken.None);
        var ids = await api.GetAccountIdsAsync([], CancellationToken.None);
        var records = await api.GetUserMapRecordsAsync([]);

        await Assert.That(names).IsEmpty();
        await Assert.That(ids).IsEmpty();
        await Assert.That(records).IsEmpty();
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetDisplayNamesAsync_ParamsOverload_ReturnsEmptyWhenApiReturnsEmptyArray()
    {
        // The API answers with [] instead of {} when none of the accounts exist.
        var fake = FakeHttpMessageHandler.Json("[]");
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());

        var names = await api.GetDisplayNamesAsync(AccountA);

        await Assert.That(names).IsSameReferenceAs(ImmutableDictionary<Guid, string>.Empty);
    }

    [Test]
    public async Task Requests_AfterExpiration_AuthorizeAgainWithSavedCredentials()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddMinutes(-1), out _),
            ValidTokenResponse,
            () => FakeResponse.Json("""{"accountId":"6a43df20-cd1a-4b3b-87b9-a6835a9b416d","displayName":"BigBang1112"}"""));
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());
        await api.AuthorizeAsync("client-id", "client-secret");

        var user = await api.GetUserAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([$"{BaseUrl}/access_token", $"{BaseUrl}/access_token", $"{BaseUrl}/user"]);
        await Assert.That(user).IsEqualTo(new User(AccountA, "BigBang1112"));
    }

    [Test]
    public async Task Requests_AfterExpiration_WithoutAutomaticAuthorization_DoNotAuthorize()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddMinutes(-1), out _),
            () => FakeResponse.Json("""{"accountId":"6a43df20-cd1a-4b3b-87b9-a6835a9b416d","displayName":"BigBang1112"}"""));
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler(), automaticallyAuthorize: false);
        await api.AuthorizeAsync("client-id", "client-secret");

        await api.GetUserAsync();

        await Assert.That(fake.Requests[1].Url).IsEqualTo($"{BaseUrl}/user");
    }

    [Test]
    public async Task GetUserAsync_Forbidden_ThrowsResponseException()
    {
        var fake = FakeHttpMessageHandler.Json("""{"message":"This action is unauthorized."}""", HttpStatusCode.Forbidden);
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler());

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => api.GetUserAsync());

        await Assert.That(ex!.Message).IsEqualTo("This action is unauthorized.");
        await Assert.That(ex.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task JwtPayload_DecodesScopes()
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);

        var payload = JwtPayloadTrackmaniaAPI.DecodeFromAccessToken(AccessToken(expiresAt));

        await Assert.That(payload.ExpirationTime).IsEqualTo(expiresAt);
        await Assert.That(payload.Subject).IsEqualTo("client-id");
        await Assert.That(payload.Scopes).IsEquivalentTo(["read_favorite"]);
    }
}
