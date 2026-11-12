using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Tests.Live;

/// <summary>
/// The client as an application gets it from dependency injection, authorizing on its first request.
/// </summary>
[LiveTest]
[NotInParallel(nameof(TrackmaniaAPI))]
public class TrackmaniaAPIHostingLiveTests
{
    private static readonly Guid AccountId = Guid.Parse("6a43df20-cd1a-4b3b-87b9-a6835a9b416d");

    [Test]
    public async Task AddTrackmaniaAPI_ClientAuthorizesOnFirstRequest()
    {
        var services = new ServiceCollection();
        services.AddTrackmaniaAPI(options => options.Credentials = new(LiveSettings.Trackmania.ClientId, LiveSettings.Trackmania.ClientSecret));

        await using var provider = services.BuildServiceProvider();

        var tm = provider.GetRequiredService<TrackmaniaAPI>();

        var displayNames = await tm.GetDisplayNamesAsync([AccountId], CancellationToken.None);

        await Assert.That(displayNames.Keys).IsEquivalentTo([AccountId]);
        await Assert.That(tm.ExpirationTime).IsNotNull();

        // The token is shared through the singleton handler, so a new client doesn't authorize again.
        var otherTm = provider.GetRequiredService<TrackmaniaAPI>();

        await Assert.That(otherTm.Handler).IsSameReferenceAs(tm.Handler);
        await Assert.That(otherTm.ExpirationTime).IsEqualTo(tm.ExpirationTime);
    }
}
