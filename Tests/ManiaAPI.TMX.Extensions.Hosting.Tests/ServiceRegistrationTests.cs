using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TMX.Extensions.Hosting.Tests;

public class ServiceRegistrationTests
{
    [Test]
    [Arguments(TmxSite.TMNF)]
    [Arguments(TmxSite.TMUF)]
    public async Task AddTMX_KeyedTmxInterface_IsSameScopedClient(TmxSite site)
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();

        var viaInterface = scope.ServiceProvider.GetRequiredKeyedService<ITMX>(site);

        await Assert.That(viaInterface).IsSameReferenceAs(scope.ServiceProvider.GetRequiredKeyedService<TMX>(site));
        await Assert.That(viaInterface.Site).IsEqualTo(site);
    }

    [Test]
    [Arguments(MxSite.Trackmania)]
    [Arguments(MxSite.Shootmania)]
    public async Task AddTMX_KeyedMxInterface_IsSameScopedClient(MxSite site)
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();

        var viaInterface = scope.ServiceProvider.GetRequiredKeyedService<IMX>(site);

        await Assert.That(viaInterface).IsSameReferenceAs(scope.ServiceProvider.GetRequiredKeyedService<MX>(site));
    }

    [Test]
    public async Task AddTMX_UnkeyedInterfaces_AreSameScopedClients()
    {
        using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        await Assert.That(services.GetRequiredService<ITMX>()).IsSameReferenceAs(services.GetRequiredService<TMX>());
        await Assert.That(services.GetRequiredService<IMX>()).IsSameReferenceAs(services.GetRequiredService<MX>());
        await Assert.That(services.GetRequiredService<IIX>()).IsSameReferenceAs(services.GetRequiredService<IX>());
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddTMX();
        return services.BuildServiceProvider();
    }
}
