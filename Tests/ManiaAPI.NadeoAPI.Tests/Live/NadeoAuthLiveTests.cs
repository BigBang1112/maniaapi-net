namespace ManiaAPI.NadeoAPI.Tests.Live;

/// <summary>
/// Authorization against the real authentication endpoints. Each test uses its own client instead of the shared ones in <see cref="LiveNadeo"/>,
/// so changing its token can't affect the other tests.
/// </summary>
[LiveTest]
[NotInParallel("Nadeo")]
[Arguments(NadeoAccount.DedicatedServer)]
[Arguments(NadeoAccount.ServiceAccount)]
public class NadeoAuthLiveTests(NadeoAccount account)
{
    [Test]
    public async Task PendingCredentials_AuthorizeOnFirstRequest_ThenRefresh()
    {
        var (login, password) = LiveNadeo.GetCredentials(account);
        var handler = new NadeoAPIHandler { PendingCredentials = new(login, password), SaveCredentials = false };
        using var nls = new NadeoLiveServices(LiveNadeo.CreateClient(), handler);

        // The first request authorizes with the pending credentials, which are then discarded.
        var clubs = await nls.GetClubsAsync(1);
        var firstToken = handler.JWT;

        await Assert.That(clubs.ClubList).IsNotEmpty();
        await Assert.That(firstToken).IsNotNull();
        await Assert.That(handler.PendingCredentials).IsNull();

        // A real token is only due for a refresh after about an hour, so pretend it already is.
        handler.JWT = firstToken! with { RefreshAt = DateTimeOffset.UtcNow.AddMinutes(-1) };

        var refreshed = await nls.RefreshAsync();
        var secondToken = handler.JWT;

        await Assert.That(refreshed).IsTrue();
        await Assert.That(secondToken!.JwtId).IsNotEqualTo(firstToken.JwtId);
        await Assert.That(secondToken.Subject).IsEqualTo(firstToken.Subject);
        await Assert.That(secondToken.RefreshAt).IsGreaterThan(DateTimeOffset.UtcNow);

        // Not due anymore, so a second call does nothing.
        await Assert.That(await nls.RefreshAsync()).IsFalse();

        // The refreshed token works.
        var clubsAfterRefresh = await nls.GetClubsAsync(1);
        await Assert.That(clubsAfterRefresh.ClubList).IsNotEmpty();
    }
}

[LiveTest]
[NotInParallel("Nadeo")]
public class NadeoAuthFailureLiveTests
{
    [Test]
    public async Task AuthorizeAsync_UnknownLogin_IsUnauthorized()
    {
        // A made-up login rather than a real one with a wrong password, so no real account can be locked for failed logins.
        // Only letters and digits, like a real login: a login with other characters gets 500 instead.
        using var ns = new NadeoServices(LiveNadeo.CreateClient());

        var ex = await Assert.ThrowsAsync<NadeoAPIResponseException>(
            () => ns.AuthorizeAsync($"nosuchlogin{Guid.NewGuid():N}"[..20], Guid.NewGuid().ToString("N")));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(ns.Handler.JWT).IsNull();
    }
}
