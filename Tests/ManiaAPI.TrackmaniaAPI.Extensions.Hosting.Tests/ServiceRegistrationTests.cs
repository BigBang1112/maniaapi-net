using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting.Tests;

public class ServiceRegistrationTests
{
    [Test]
    public async Task AddTrackmaniaAPI_RegistersInterface()
    {
        var services = new ServiceCollection();
        services.AddTrackmaniaAPI();
        using var provider = services.BuildServiceProvider();

        var api = provider.GetRequiredService<ITrackmaniaAPI>();

        await Assert.That(api).IsTypeOf<TrackmaniaAPI>();
        await Assert.That(((TrackmaniaAPI)api).Handler).IsSameReferenceAs(provider.GetRequiredService<TrackmaniaAPIHandler>());
    }
}
