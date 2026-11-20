namespace ManiaAPI.TMX.Tests;

public class UserAgentTests
{
    [Test]
    public async Task TMX_0_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new TMX(TmxSite.TMUF, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new TMX(http, TmxSite.TMUF, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new TMX(http, TmxSite.TMUF, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task MX_1_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new MX(MxSite.Trackmania, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new MX(http, MxSite.Trackmania, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new MX(http, MxSite.Trackmania, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task IX_2_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new IX(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new IX(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new IX(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
