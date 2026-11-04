namespace ManiaAPI.TrackmaniaAPI.Tests.Live;

[Category("Live"), LiveTest, Retry(2)]
[NotInParallel(nameof(TrackmaniaAPI))]
public class TrackmaniaAPILiveTests
{
    private static readonly Guid[] AccountIds =
    [
        Guid.Parse("6a43df20-cd1a-4b3b-87b9-a6835a9b416d"),
        Guid.Parse("faedcf21-d61a-4305-9ffe-680b2ee5d65e")
    ];

    private static async Task<TrackmaniaAPI> AuthorizeAsync()
    {
        var clientId = LiveSecrets.Require("ClientId", "TRACKMANIA_API_CLIENTID");
        var clientSecret = LiveSecrets.Require("ClientSecret", "TRACKMANIA_API_CLIENTSECRET");

        var api = new TrackmaniaAPI();
        await api.AuthorizeAsync(clientId, clientSecret, ["read_favorite"]);
        return api;
    }

    [Test]
    public async Task DisplayNames_RoundTripToAccountIds()
    {
        using var api = await AuthorizeAsync();

        var displayNames = await api.GetDisplayNamesAsync(AccountIds, CancellationToken.None);
        var accountIds = await api.GetAccountIdsAsync(displayNames.Values, CancellationToken.None);

        await Assert.That(displayNames.Keys).IsEquivalentTo(AccountIds);
        await Assert.That(accountIds.Values).IsEquivalentTo(AccountIds);
    }

    [Test]
    public async Task GetUserAsync_WithClientCredentials_IsRejected()
    {
        using var api = await AuthorizeAsync();

        // Only a user's authorization code token can access the user endpoints.
        await Assert.ThrowsAsync<TrackmaniaAPIResponseException>(() => api.GetUserAsync());
    }
}
