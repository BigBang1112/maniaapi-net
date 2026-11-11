using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaAPI.Tests;

public class TrackmaniaAPITests
{
    private const string BaseUrl = "https://api.trackmania.com/api";
    private const string ClientId = "client-id";
    private const string ClientSecret = "client-secret";
    private const string Scope = "read_favorite";
    private const string DisplayName = "BigBang1112";
    private const string FavoriteMapUid = "JlxlB7KbrCfhjAf5ld89ByXR987";

    private const string AdminClubsJson = """{"clubs":[{"id":12730,"name":"TM SCENERY HUB"}]}""";

    private const string FavoriteMapsJson = """
        {
          "list": [
            {
              "uid": "JlxlB7KbrCfhjAf5ld89ByXR987",
              "name": "Winter 2022 - 01",
              "author": "d2372a08-a8a1-46cb-97fb-23a161d85ad0",
              "thumbnailUrl": "https://prod.trackmania.core.nadeo.online/storageObjects/3ddb465d-0357-41c0-9946-394b96836577.jpg"
            }
          ]
        }
        """;

    private const string InvalidClientJson = """{"error":"invalid_client","message":"Client authentication failed","error_description":"Client authentication failed"}""";

    private static readonly Guid AccountA = Guid.Parse("6a43df20-cd1a-4b3b-87b9-a6835a9b416d");
    private static readonly Guid AccountB = Guid.Parse("faedcf21-d61a-4305-9ffe-680b2ee5d65e");
    private static readonly DateTimeOffset ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(1_900_000_000);

    private static readonly string UserJson = $$"""{"accountId":"{{AccountA}}","displayName":"{{DisplayName}}"}""";

    [Test]
    public async Task AuthorizeAsync_PostsClientCredentials()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(ExpiresAt, out _));
        using var api = CreateApi(fake);

        await api.AuthorizeAsync(ClientId, ClientSecret, [Scope]);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/access_token");
        await Assert.That(fake.Request.Headers["Content-Type"]).StartsWith("application/x-www-form-urlencoded");
        await Assert.That(fake.Request.Body).IsEqualTo($"grant_type=client_credentials&client_id={ClientId}&client_secret={ClientSecret}&scope={Scope}");
    }

    [Test]
    public async Task AuthorizeAsync_MultipleScopes_PostsSpaceSeparatedScopes()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(ExpiresAt, out _));
        using var api = CreateApi(fake);

        await api.AuthorizeAsync(ClientId, ClientSecret, ["clubs", "read_favorite"]);

        await Assert.That(fake.Request.Body).IsEqualTo($"grant_type=client_credentials&client_id={ClientId}&client_secret={ClientSecret}&scope=clubs+read_favorite");
    }

    [Test]
    public async Task AuthorizeAsync_ValidToken_StoresTokenClaims()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(ExpiresAt, out _));
        using var api = CreateApi(fake);

        await api.AuthorizeAsync(ClientId, ClientSecret, [Scope]);

        await Assert.That(api.ExpirationTime).IsEqualTo(ExpiresAt);
        await Assert.That(api.Handler.Payload!.Scopes).IsEquivalentTo([Scope]);
    }

    [Test]
    public async Task AuthorizeAsync_ValidToken_SavesCredentials()
    {
        var fake = FakeHttpMessageHandler.Sequence(() => TokenResponse(ExpiresAt, out _));
        using var api = CreateApi(fake);

        await api.AuthorizeAsync(ClientId, ClientSecret);

        await Assert.That(api.Handler.Credentials?.ClientId).IsEqualTo(ClientId);
    }

    [Test]
    public async Task AuthorizeAsync_InvalidClient_ThrowsWithErrorDescription()
    {
        var fake = FakeHttpMessageHandler.Json(InvalidClientJson, HttpStatusCode.Unauthorized);
        using var api = CreateApi(fake);

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => api.AuthorizeAsync(ClientId, "wrong"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(ex.Message).IsEqualTo("Client authentication failed");
        await Assert.That(ex.Response?.Error).IsEqualTo("invalid_client");
    }

    [Test]
    public async Task AuthorizeAsync_InvalidClient_DoesNotSaveCredentials()
    {
        var fake = FakeHttpMessageHandler.Json(InvalidClientJson, HttpStatusCode.Unauthorized);
        using var api = CreateApi(fake);

        await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => api.AuthorizeAsync(ClientId, "wrong"));

        await Assert.That(api.Handler.Credentials).IsNull();
    }

    [Test]
    public async Task GetDisplayNamesAsync_SendsIndexedAccountIds()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var api = CreateApi(fake);

        await api.GetDisplayNamesAsync([AccountA, AccountB], CancellationToken.None);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/display-names?accountId[0]={AccountA}&accountId[1]={AccountB}");
    }

    [Test]
    public async Task GetDisplayNamesAsync_AfterAuthorization_SendsBearerToken()
    {
        var accessToken = "";
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddHours(1), out accessToken),
            () => FakeResponse.Json("{}"));
        using var api = CreateApi(fake);
        await api.AuthorizeAsync(ClientId, ClientSecret);

        await api.GetDisplayNamesAsync([AccountA], CancellationToken.None);

        await Assert.That(fake.Requests[1].Authorization?.Scheme).IsEqualTo("Bearer");
        await Assert.That(fake.Requests[1].Authorization?.Parameter).IsEqualTo(accessToken);
    }

    [Test]
    public async Task GetDisplayNamesAsync_ReturnsNamesByAccountId()
    {
        var fake = FakeHttpMessageHandler.Json($$"""{"{{AccountA}}":"{{DisplayName}}","{{AccountB}}":"Player"}""");
        using var api = CreateApi(fake);

        var names = await api.GetDisplayNamesAsync([AccountA, AccountB], CancellationToken.None);

        await Assert.That(names).IsEquivalentTo(new Dictionary<Guid, string> { [AccountA] = DisplayName, [AccountB] = "Player" });
    }

    [Test]
    public async Task GetDisplayNamesAsync_NoAccountIds_ReturnsEmptyWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var api = CreateApi(fake);

        var names = await api.GetDisplayNamesAsync([], CancellationToken.None);

        await Assert.That(names).IsEmpty();
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetDisplayNamesAsync_ApiReturnsEmptyArray_ReturnsEmpty()
    {
        // The API answers with [] instead of {} when none of the accounts exist.
        var fake = FakeHttpMessageHandler.Json("[]");
        using var api = CreateApi(fake);

        var names = await api.GetDisplayNamesAsync(AccountA);

        await Assert.That(names).IsSameReferenceAs(ImmutableDictionary<Guid, string>.Empty);
    }

    [Test]
    public async Task GetAccountIdsAsync_SendsIndexedDisplayNames()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var api = CreateApi(fake);

        await api.GetAccountIdsAsync([DisplayName], CancellationToken.None);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/display-names/account-ids?displayName[0]={DisplayName}");
    }

    [Test]
    public async Task GetAccountIdsAsync_ReturnsAccountIdsByDisplayName()
    {
        var fake = FakeHttpMessageHandler.Json($$"""{"{{DisplayName}}":"{{AccountA}}"}""");
        using var api = CreateApi(fake);

        var ids = await api.GetAccountIdsAsync([DisplayName], CancellationToken.None);

        await Assert.That(ids).IsEquivalentTo(new Dictionary<string, Guid> { [DisplayName] = AccountA });
    }

    [Test]
    public async Task GetAccountIdsAsync_NoDisplayNames_ReturnsEmptyWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var api = CreateApi(fake);

        var ids = await api.GetAccountIdsAsync([], CancellationToken.None);

        await Assert.That(ids).IsEmpty();
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetUserMapRecordsAsync_NoMapIds_ReturnsEmptyWithoutRequest()
    {
        var fake = FakeHttpMessageHandler.Json("{}");
        using var api = CreateApi(fake);

        var records = await api.GetUserMapRecordsAsync([]);

        await Assert.That(records).IsEmpty();
        await Assert.That(fake.Requests).IsEmpty();
    }

    [Test]
    public async Task GetUserAsync_ReturnsUser()
    {
        var fake = FakeHttpMessageHandler.Json(UserJson);
        using var api = CreateApi(fake);

        var user = await api.GetUserAsync();

        await Assert.That(user).IsEqualTo(new User(AccountA, DisplayName));
    }

    [Test]
    public async Task GetUserAsync_TokenExpired_AuthorizesAgainWithSavedCredentials()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddMinutes(-1), out _),
            () => TokenResponse(DateTimeOffset.UtcNow.AddHours(1), out _),
            () => FakeResponse.Json(UserJson));
        using var api = CreateApi(fake);
        await api.AuthorizeAsync(ClientId, ClientSecret);

        await api.GetUserAsync();

        await Assert.That(fake.Requests.Select(x => x.Url)).IsEquivalentTo([$"{BaseUrl}/access_token", $"{BaseUrl}/access_token", $"{BaseUrl}/user"]);
    }

    [Test]
    public async Task GetUserAsync_TokenExpiredWithoutAutomaticAuthorization_DoesNotAuthorize()
    {
        var fake = FakeHttpMessageHandler.Sequence(
            () => TokenResponse(DateTimeOffset.UtcNow.AddMinutes(-1), out _),
            () => FakeResponse.Json(UserJson));
        using var api = new TrackmaniaAPI(fake.CreateClient(), new TrackmaniaAPIHandler(), automaticallyAuthorize: false);
        await api.AuthorizeAsync(ClientId, ClientSecret);

        await api.GetUserAsync();

        await Assert.That(fake.Requests[1].Url).IsEqualTo($"{BaseUrl}/user");
    }

    [Test]
    public async Task GetUserAsync_Forbidden_ThrowsResponseException()
    {
        var fake = FakeHttpMessageHandler.Json("""{"message":"This action is unauthorized."}""", HttpStatusCode.Forbidden);
        using var api = CreateApi(fake);

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => api.GetUserAsync());

        await Assert.That(ex!.Message).IsEqualTo("This action is unauthorized.");
        await Assert.That(ex.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DecodeFromAccessToken_ReturnsClaims()
    {
        var payload = JwtPayloadTrackmaniaAPI.DecodeFromAccessToken(AccessToken(ExpiresAt));

        await Assert.That(payload.ExpirationTime).IsEqualTo(ExpiresAt);
        await Assert.That(payload.Subject).IsEqualTo(ClientId);
        await Assert.That(payload.Scopes).IsEquivalentTo([Scope]);
    }

    [Test]
    public async Task GetUserAdminClubsAsync_RequestsAdminClubs()
    {
        var fake = FakeHttpMessageHandler.Json(AdminClubsJson);
        using var api = CreateApi(fake);

        await api.GetUserAdminClubsAsync();

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/user/clubs/admin");
    }

    [Test]
    public async Task GetUserAdminClubsAsync_ReturnsClubs()
    {
        var fake = FakeHttpMessageHandler.Json(AdminClubsJson);
        using var api = CreateApi(fake);

        var clubs = await api.GetUserAdminClubsAsync();

        await Assert.That(clubs).IsEquivalentTo([new AdminClub(12730, "TM SCENERY HUB")]);
    }

    [Test]
    public async Task GetUserFavoriteMapsAsync_AddsLengthAndOffset()
    {
        var fake = FakeHttpMessageHandler.Json(FavoriteMapsJson);
        using var api = CreateApi(fake);

        await api.GetUserFavoriteMapsAsync(length: 1, offset: 3);

        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/user/maps/favorite?length=1&offset=3");
    }

    [Test]
    public async Task GetUserFavoriteMapsAsync_ReturnsMaps()
    {
        var fake = FakeHttpMessageHandler.Json(FavoriteMapsJson);
        using var api = CreateApi(fake);

        var maps = await api.GetUserFavoriteMapsAsync();

        await Assert.That(maps).IsEquivalentTo([new FavoriteMap(
            FavoriteMapUid,
            "Winter 2022 - 01",
            Guid.Parse("d2372a08-a8a1-46cb-97fb-23a161d85ad0"),
            "https://prod.trackmania.core.nadeo.online/storageObjects/3ddb465d-0357-41c0-9946-394b96836577.jpg")]);
    }

    [Test]
    public async Task AddUserFavoriteMapAsync_PostsUidToAdd()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NoContent));
        using var api = CreateApi(fake);

        await api.AddUserFavoriteMapAsync(FavoriteMapUid);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/user/maps/favorite/add");
        await Assert.That(fake.Request.Body).IsEqualTo($$"""{"uid":"{{FavoriteMapUid}}"}""");
    }

    [Test]
    public async Task AddUserFavoriteMapAsync_Forbidden_ThrowsResponseException()
    {
        var fake = FakeHttpMessageHandler.Json("""{"message":"This action is unauthorized."}""", HttpStatusCode.Forbidden);
        using var api = CreateApi(fake);

        await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => api.AddUserFavoriteMapAsync(FavoriteMapUid));
    }

    [Test]
    public async Task RemoveUserFavoriteMapAsync_PostsUidToRemove()
    {
        var fake = new FakeHttpMessageHandler(_ => FakeResponse.Status(HttpStatusCode.NoContent));
        using var api = CreateApi(fake);

        await api.RemoveUserFavoriteMapAsync(FavoriteMapUid);

        await Assert.That(fake.Request.Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(fake.Request.Url).IsEqualTo($"{BaseUrl}/user/maps/favorite/remove");
        await Assert.That(fake.Request.Body).IsEqualTo($$"""{"uid":"{{FavoriteMapUid}}"}""");
    }

    private static TrackmaniaAPI CreateApi(FakeHttpMessageHandler fake) => new(fake.CreateClient(), new TrackmaniaAPIHandler());

    private static string AccessToken(DateTimeOffset expiresAt) => TestJwt.Create($$"""
        {
          "jti": "token-id",
          "aud": "{{ClientId}}",
          "iat": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
          "nbf": {{expiresAt.AddHours(-1).ToUnixTimeSeconds()}},
          "exp": {{expiresAt.ToUnixTimeSeconds()}},
          "sub": "{{ClientId}}",
          "scopes": ["{{Scope}}"]
        }
        """);

    private static HttpResponseMessage TokenResponse(DateTimeOffset expiresAt, out string accessToken)
    {
        accessToken = AccessToken(expiresAt);
        return FakeResponse.Json($$"""{"token_type":"Bearer","expires_in":3600,"access_token":"{{accessToken}}"}""");
    }
}
