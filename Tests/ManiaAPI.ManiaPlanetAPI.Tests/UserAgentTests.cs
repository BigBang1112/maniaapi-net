namespace ManiaAPI.ManiaPlanetAPI.Tests;

public class UserAgentTests
{
    [Test]
    public async Task ManiaPlanetAPI_0_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new ManiaPlanetAPI(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new ManiaPlanetAPI(http, new ManiaPlanetAPIHandler(), UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new ManiaPlanetAPI(http, new ManiaPlanetAPIHandler(), UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task ManiaPlanetIngameAPI_1_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new ManiaPlanetIngameAPI(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new ManiaPlanetIngameAPI(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new ManiaPlanetIngameAPI(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task ManiaPlanetChannelRecordAPI_2_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new ManiaPlanetChannelRecordAPI(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new ManiaPlanetChannelRecordAPI(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new ManiaPlanetChannelRecordAPI(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task ManiaPlanetChannelFeedbackAPI_3_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new ManiaPlanetChannelFeedbackAPI(UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new ManiaPlanetChannelFeedbackAPI(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new ManiaPlanetChannelFeedbackAPI(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
