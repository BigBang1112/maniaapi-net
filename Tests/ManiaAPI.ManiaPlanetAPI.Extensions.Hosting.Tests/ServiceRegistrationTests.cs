using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting.Tests;

public class ServiceRegistrationTests
{
    [Test]
    public async Task AddManiaPlanetClients_RegisterInterfaces()
    {
        var services = new ServiceCollection();
        services.AddManiaPlanetAPI();
        services.AddManiaPlanetIngameAPI();
        services.AddManiaPlanetChannelRecordAPI();
        services.AddManiaPlanetChannelFeedbackAPI();
        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<IManiaPlanetAPI>()).IsTypeOf<ManiaPlanetAPI>();
        await Assert.That(provider.GetRequiredService<IManiaPlanetIngameAPI>()).IsTypeOf<ManiaPlanetIngameAPI>();
        await Assert.That(provider.GetRequiredService<IManiaPlanetChannelRecordAPI>()).IsTypeOf<ManiaPlanetChannelRecordAPI>();
        await Assert.That(provider.GetRequiredService<IManiaPlanetChannelFeedbackAPI>()).IsTypeOf<ManiaPlanetChannelFeedbackAPI>();
    }
}
