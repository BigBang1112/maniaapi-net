namespace ManiaAPI.TrackmaniaAPI.Tests;

public class UserAgentTests
{
    [Test]
    public async Task TrackmaniaAPI_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new TrackmaniaAPI(UserAgentAssertions.Value, automaticallyAuthorize: false).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new TrackmaniaAPI(http, new TrackmaniaAPIHandler(), UserAgentAssertions.Value, automaticallyAuthorize: false);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new TrackmaniaAPI(http, new TrackmaniaAPIHandler(), UserAgentAssertions.Value, automaticallyAuthorize: false);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
