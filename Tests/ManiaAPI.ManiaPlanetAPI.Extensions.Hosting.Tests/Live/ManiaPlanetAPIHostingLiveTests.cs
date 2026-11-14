using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Tests.Live;

/// <summary>
/// The clients as an application gets them from dependency injection.
/// </summary>
[LiveTest]
[NotInParallel("ManiaPlanet")]
public class ManiaPlanetAPIHostingLiveTests
{
    [Test]
    public async Task AddManiaPlanetAPI_ClientAuthorizesOnFirstRequest()
    {
        var services = new ServiceCollection();
        services.AddManiaPlanetAPI(options => options.Credentials = new(LiveSettings.ManiaPlanet.ClientId, LiveSettings.ManiaPlanet.ClientSecret));

        await using var provider = services.BuildServiceProvider();

        var mp = provider.GetRequiredService<ManiaPlanetAPI>();

        var zones = await mp.GetZonesAsync();

        await Assert.That(zones).Contains("World");
        await Assert.That(mp.ExpirationTime).IsNotNull();
    }

    [Test]
    public async Task AddManiaPlanetIngameAPI_ClientHasBaseAddress()
    {
        var services = new ServiceCollection();
        services.AddManiaPlanetIngameAPI();

        await using var provider = services.BuildServiceProvider();

        var ingame = provider.GetRequiredService<ManiaPlanetIngameAPI>();

        var title = await ingame.GetTitleByUidAsync("TMStadium@nadeo");

        await Assert.That(title?.Uid).IsEqualTo("TMStadium@nadeo");
    }
}
