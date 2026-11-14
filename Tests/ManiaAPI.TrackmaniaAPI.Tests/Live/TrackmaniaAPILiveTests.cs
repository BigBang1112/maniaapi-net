namespace ManiaAPI.TrackmaniaAPI.Tests.Live;

/// <summary>
/// Live tests with a token from the client credentials flow, the only kind of token the live tests can get without a player logging in.
/// The endpoints for a player (<c>/user/...</c>) need a token from the authorization code flow, so here they are checked to be rejected.
/// </summary>
[LiveTest]
[NotInParallel(nameof(TrackmaniaAPI))]
public class TrackmaniaAPILiveTests
{
    /// <summary>
    /// Any existing map works: the favorite requests are rejected before the map is looked at.
    /// </summary>
    private const string FavoriteMapUid = "JlxlB7KbrCfhjAf5ld89ByXR987";

    /// <summary>
    /// The most account IDs or display names one request accepts. The API answers 400 to more.
    /// </summary>
    private const int MaxNamesPerRequest = 50;

    private static readonly string[] Scopes = ["clubs", "read_favorite", "write_favorite"];

    /// <summary>
    /// Two player accounts that exist and have display names, one of them the library author's.
    /// </summary>
    private static readonly Guid[] AccountIds =
    [
        Guid.Parse("6a43df20-cd1a-4b3b-87b9-a6835a9b416d"),
        Guid.Parse("faedcf21-d61a-4305-9ffe-680b2ee5d65e")
    ];

    /// <summary>
    /// A map of an official campaign, so it won't be deleted.
    /// </summary>
    private static readonly Guid MapId = Guid.Parse("ed53486f-a80d-419b-b073-54bb8944d502");

    // One token for every test, so the live tests don't request a new one each time.
    private static readonly Lazy<Task<TrackmaniaAPI>> api = new(AuthorizeAsync);

    private static async Task<TrackmaniaAPI> AuthorizeAsync()
    {
        var api = new TrackmaniaAPI();
        await api.AuthorizeAsync(LiveSettings.Trackmania.ClientId, LiveSettings.Trackmania.ClientSecret, Scopes);
        return api;
    }

    [Test]
    public async Task AuthorizeAsync_MultipleScopes_GrantsAllOfThem()
    {
        var tm = await api.Value;

        await Assert.That(tm.Handler.Payload!.Scopes).IsEquivalentTo(Scopes);
        await Assert.That(tm.ExpirationTime!.Value).IsGreaterThan(DateTimeOffset.UtcNow);
    }

    [Test]
    public async Task AuthorizeAsync_UnknownClient_IsUnauthorized()
    {
        using var tm = new TrackmaniaAPI();

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(
            () => tm.AuthorizeAsync($"no-such-client-{Guid.NewGuid():N}", Guid.NewGuid().ToString("N")));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(tm.Handler.Credentials).IsNull();
    }

    [Test]
    public async Task DisplayNames_RoundTripToAccountIds()
    {
        var tm = await api.Value;

        var displayNames = await tm.GetDisplayNamesAsync(AccountIds, CancellationToken.None);
        var accountIds = await tm.GetAccountIdsAsync(displayNames.Values, CancellationToken.None);

        await Assert.That(displayNames.Keys).IsEquivalentTo(AccountIds);
        await Assert.That(accountIds.Values).IsEquivalentTo(AccountIds);
    }

    [Test]
    public async Task GetDisplayNamesAsync_UnknownAccount_ReturnsEmpty()
    {
        // The API answers [] instead of {} when none of the accounts exist.
        // The overload with a CancellationToken, because the params one used to hide a failure to read [].
        var tm = await api.Value;

        var displayNames = await tm.GetDisplayNamesAsync([Guid.NewGuid()], CancellationToken.None);

        await Assert.That(displayNames).IsEmpty();
    }

    [Test]
    public async Task GetDisplayNamesAsync_KnownAndUnknownAccounts_ReturnsOnlyKnown()
    {
        var tm = await api.Value;

        var displayNames = await tm.GetDisplayNamesAsync(AccountIds[0], Guid.NewGuid());

        await Assert.That(displayNames.Keys).IsEquivalentTo([AccountIds[0]]);
    }

    [Test]
    public async Task GetAccountIdsAsync_UnknownDisplayName_ReturnsEmpty()
    {
        // The API answers [] instead of {} when none of the display names exist.
        // The overload with a CancellationToken, because the params one used to hide a failure to read [].
        var tm = await api.Value;

        var accountIds = await tm.GetAccountIdsAsync([$"no-such-player-{Guid.NewGuid():N}"[..24]], CancellationToken.None);

        await Assert.That(accountIds).IsEmpty();
    }

    [Test]
    public async Task GetDisplayNamesAsync_MoreThanMaxAccounts_IsRejected()
    {
        var tm = await api.Value;
        var accountIds = Enumerable.Range(0, MaxNamesPerRequest + 1).Select(_ => Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => tm.GetDisplayNamesAsync(accountIds, CancellationToken.None));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetAccountIdsAsync_MoreThanMaxDisplayNames_IsRejected()
    {
        var tm = await api.Value;
        var displayNames = Enumerable.Range(0, MaxNamesPerRequest + 1).Select(i => $"no-such-player-{i}");

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => tm.GetAccountIdsAsync(displayNames, CancellationToken.None));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUserAsync_WithClientCredentials_IsRejected()
    {
        // The API answers 500 ("Current user is not an OAuthUser") rather than 401, so only the rejection is checked.
        var tm = await api.Value;

        await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => tm.GetUserAsync());
    }

    [Test]
    [Arguments("user/clubs/admin")]
    [Arguments("user/maps/favorite")]
    [Arguments("user/maps/favorite/add")]
    [Arguments("user/maps/favorite/remove")]
    [Arguments("user/map-records")]
    public async Task PlayerEndpoint_WithClientCredentials_IsUnauthorized(string endpoint)
    {
        var tm = await api.Value;

        Func<Task> request = endpoint switch
        {
            "user/clubs/admin" => () => tm.GetUserAdminClubsAsync(),
            "user/maps/favorite" => () => tm.GetUserFavoriteMapsAsync(),
            // There is no player behind the token, so these can't change any favorites.
            "user/maps/favorite/add" => () => tm.AddUserFavoriteMapAsync(FavoriteMapUid),
            "user/maps/favorite/remove" => () => tm.RemoveUserFavoriteMapAsync(FavoriteMapUid),
            "user/map-records" => () => tm.GetUserMapRecordsAsync([MapId]),
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint))
        };

        var ex = await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(request);

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
