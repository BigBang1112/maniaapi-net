namespace ManiaAPI.Dedimania.Tests;

public class UserAgentTests
{
    [Test]
    public async Task Dedimania_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new Dedimania(DedimaniaGame.TMF, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new Dedimania(http, DedimaniaGame.TMF, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new Dedimania(http, DedimaniaGame.TMF, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
