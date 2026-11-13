namespace ManiaAPI.TrackmaniaWS.Tests.Live;

/// <summary>
/// There are no credentials for the TrackmaniaWS API in the live test settings, so this only checks that the service is up
/// and that wrong credentials are reported as such.
/// </summary>
[LiveTest]
public class TrackmaniaWSLiveTests
{
    [Test]
    public async Task PingAsync_ServiceIsUp()
    {
        // The test route needs no credentials, so any are fine
        using var ws = new TrackmaniaWS("", "");

        await Assert.That(await ws.PingAsync()).IsTrue();
    }

    [Test]
    public async Task GetPlayerAsync_WrongCredentials_IsUnauthorized()
    {
        using var ws = new TrackmaniaWS($"nosuchuser{Guid.NewGuid():N}"[..20], Guid.NewGuid().ToString("N"));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => ws.GetPlayerAsync("bigbang1112"));

        await Assert.That(ex!.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
