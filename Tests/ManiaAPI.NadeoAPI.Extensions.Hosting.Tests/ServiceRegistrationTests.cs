using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting.Tests;

public class ServiceRegistrationTests
{
    [Test]
    public async Task AddNadeoAPI_RegistersInterfacesOfEveryService()
    {
        var services = new ServiceCollection();
        services.AddNadeoAPI();
        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<INadeoServices>()).IsTypeOf<NadeoServices>();
        await Assert.That(provider.GetRequiredService<INadeoLiveServices>()).IsTypeOf<NadeoLiveServices>();
        await Assert.That(provider.GetRequiredService<INadeoMeetServices>()).IsTypeOf<NadeoMeetServices>();
    }

    [Test]
    public async Task AddNadeoAPI_InterfaceSharesHandlerWithConcreteClient()
    {
        var services = new ServiceCollection();
        services.AddNadeoAPI();
        using var provider = services.BuildServiceProvider();

        var viaInterface = (NadeoLiveServices)provider.GetRequiredService<INadeoLiveServices>();
        var concrete = provider.GetRequiredService<NadeoLiveServices>();

        // Clients are transient, but they must share one handler so they share the authorization
        await Assert.That(viaInterface.Handler).IsSameReferenceAs(concrete.Handler);
    }

    [Test]
    public async Task AddNadeoServicesSeparately_RegistersEachInterface()
    {
        var services = new ServiceCollection();
        services.AddNadeoServices(_ => { });
        services.AddNadeoLiveServices(_ => { });
        services.AddNadeoMeetServices(_ => { });
        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<INadeoServices>()).IsTypeOf<NadeoServices>();
        await Assert.That(provider.GetRequiredService<INadeoLiveServices>()).IsTypeOf<NadeoLiveServices>();
        await Assert.That(provider.GetRequiredService<INadeoMeetServices>()).IsTypeOf<NadeoMeetServices>();
    }
}
