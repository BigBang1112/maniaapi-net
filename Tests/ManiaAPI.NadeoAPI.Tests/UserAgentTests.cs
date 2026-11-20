namespace ManiaAPI.NadeoAPI.Tests;

public class UserAgentTests
{
    [Test]
    public async Task NadeoServices_0_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new NadeoServices(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new NadeoServices(http, null, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new NadeoServices(http, null, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task NadeoLiveServices_1_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new NadeoLiveServices(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new NadeoLiveServices(http, null, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new NadeoLiveServices(http, null, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task NadeoMeetServices_2_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new NadeoMeetServices(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new NadeoMeetServices(http, null, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new NadeoMeetServices(http, null, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
