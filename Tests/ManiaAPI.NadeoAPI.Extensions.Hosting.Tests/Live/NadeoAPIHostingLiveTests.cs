using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting.Tests.Live;

/// <summary>
/// The clients as an application gets them from dependency injection, authorizing on their first request.
/// </summary>
[LiveTest]
[NotInParallel("NadeoAPI")]
public class NadeoAPIHostingLiveTests
{
    [Test]
    public async Task AddNadeoAPI_ClientsAuthorizeOnFirstRequest()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNadeoAPI(options =>
        {
            options.Credentials = new(LiveSettings.NadeoAPI.ServiceAccount.Login, LiveSettings.NadeoAPI.ServiceAccount.Password);
            options.UserAgent = "ManiaAPI.NadeoAPI.Extensions.Hosting.Tests/1.0";
        });

        await using var provider = services.BuildServiceProvider();

        var ns = provider.GetRequiredService<NadeoServices>();
        var nls = provider.GetRequiredService<NadeoLiveServices>();
        var nms = provider.GetRequiredService<NadeoMeetServices>();

        var zones = await ns.GetZonesAsync();
        var clubs = await nls.GetClubsAsync(1);
        var summary = await nms.GetMatchmakingSummaryAsync();

        await Assert.That(zones).IsNotEmpty();
        await Assert.That(clubs.ClubList).IsNotEmpty();
        await Assert.That(summary.Ranked3v3Id).IsGreaterThan(0);

        // The clients are transient, but each service's token is shared through its handler, so a new client doesn't authorize again.
        var otherNls = provider.GetRequiredService<NadeoLiveServices>();

        await Assert.That(otherNls).IsNotSameReferenceAs(nls);
        await Assert.That(otherNls.Handler).IsSameReferenceAs(nls.Handler);
        await Assert.That(otherNls.Handler.JWT?.Audience).IsEqualTo("NadeoLiveServices");
        await Assert.That(otherNls.Handler.PendingCredentials).IsNull();
        await Assert.That(otherNls.Client.DefaultRequestHeaders.UserAgent.ToString()).Contains("ManiaAPI.NadeoAPI.Extensions.Hosting.Tests/1.0");
    }
}
