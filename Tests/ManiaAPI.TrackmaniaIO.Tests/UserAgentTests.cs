namespace ManiaAPI.TrackmaniaIO.Tests;

public class UserAgentTests
{
    [Test]
    public async Task TrackmaniaIO_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new TrackmaniaIO(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new TrackmaniaIO(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new TrackmaniaIO(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
