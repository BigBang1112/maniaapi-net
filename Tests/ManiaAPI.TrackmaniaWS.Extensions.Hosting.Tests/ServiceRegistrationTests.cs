using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaWS.Extensions.Hosting.Tests;

public class ServiceRegistrationTests
{
    [Test]
    public async Task AddTrackmaniaWS_RegistersInterface()
    {
        var services = new ServiceCollection();
        services.AddTrackmaniaWS(new TrackmaniaWSOptions { Credentials = new("username", "password") });
        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<ITrackmaniaWS>()).IsTypeOf<TrackmaniaWS>();
    }
}
