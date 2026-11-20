using ManiaAPI.MasterServer.MP3;
using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMT;
using ManiaAPI.MasterServer.TMUF;

namespace ManiaAPI.MasterServer.Tests;

public class UserAgentTests
{
    [Test]
    public async Task MasterServerTMUF_0_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new MasterServerTMUF(MasterServerType.United, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new MasterServerTMUF(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new MasterServerTMUF(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task MasterServerMP3_1_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new MasterServerMP3(MasterServerMP3.DefaultUrl, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new MasterServerMP3(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new MasterServerMP3(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task InitServerMP3_2_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new InitServerMP3(InitServerMP3.DefaultUrl, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new InitServerMP3(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new InitServerMP3(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task MasterServerMP4_3_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new MasterServerMP4(MasterServerMP4.DefaultUrl, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new MasterServerMP4(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new MasterServerMP4(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task InitServerMP4_4_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new InitServerMP4(InitServerMP4.DefaultUrl, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new InitServerMP4(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new InitServerMP4(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task MasterServerTMT_5_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new MasterServerTMT(new Uri("https://example.test/"), UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new MasterServerTMT(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new MasterServerTMT(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }

    [Test]
    public async Task InitServerTMT_6_ApplicationIdentityAppearsFirst()
    {
        using var fresh = new InitServerTMT(Platform.PC, UserAgentAssertions.Value).Client;
        await UserAgentAssertions.VerifyAsync(fresh);

        using var http = UserAgentAssertions.CreateClient();
        _ = new InitServerTMT(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);

        // Wrapping a shared HttpClient again must not duplicate either identity.
        _ = new InitServerTMT(http, UserAgentAssertions.Value);
        await UserAgentAssertions.VerifyAsync(http, replacesExisting: true);
    }
}
