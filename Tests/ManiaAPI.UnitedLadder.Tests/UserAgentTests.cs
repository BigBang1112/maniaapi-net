namespace ManiaAPI.UnitedLadder.Tests;

public class UserAgentTests
{
    [Test]
    public async Task UnitedLadder_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new UnitedLadder(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new UnitedLadder(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new UnitedLadder(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
