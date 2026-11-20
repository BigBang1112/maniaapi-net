namespace ManiaAPI.TrackmaniaWS.Tests;

public class UserAgentTests
{
    [Test]
    public async Task TrackmaniaWS_0_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new TrackmaniaWS("login", "password", UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new TrackmaniaWS("login", "password", http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new TrackmaniaWS("login", "password", http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task TrackmaniaWS_1_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new TrackmaniaWS(new TrackmaniaWSCredentials("login", "password"), UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new TrackmaniaWS(new TrackmaniaWSCredentials("login", "password"), http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new TrackmaniaWS(new TrackmaniaWSCredentials("login", "password"), http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
