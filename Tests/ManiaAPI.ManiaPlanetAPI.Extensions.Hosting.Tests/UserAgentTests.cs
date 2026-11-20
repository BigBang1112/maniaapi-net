using ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Tests;

public class UserAgentTests
{
    [Test]
    public async Task OAuthBackchannel_UsesApplicationIdentityForEachScheme()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication()
            .AddManiaPlanet("first", options =>
            {
                options.ClientId = "client";
                options.ClientSecret = "secret";
                options.UserAgent = UserAgentAssertions.Value;
                options.Backchannel = UserAgentAssertions.CreateClient();
            })
            .AddManiaPlanet("second", options =>
            {
                options.ClientId = "client";
                options.ClientSecret = "secret";
                options.UserAgent = "SecondApp/2.0";
            });
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<ManiaPlanetAuthenticationOptions>>();
        using var first = options.Get("first").Backchannel;
        using var second = options.Get("second").Backchannel;
        await Assert.That(first.DefaultRequestHeaders.UserAgent.ToString()).StartsWith(UserAgentAssertions.Value);
        await Assert.That(first.DefaultRequestHeaders.UserAgent.ToString()).DoesNotContain("ExistingApp/2.0");
        await Assert.That(second.DefaultRequestHeaders.UserAgent.ToString()).StartsWith("SecondApp/2.0");
        await Assert.That(second.DefaultRequestHeaders.UserAgent.ToString()).DoesNotContain("MyApp/1.0");
    }
}
