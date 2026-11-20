using ManiaAPI.MasterServer.MP3;
using ManiaAPI.MasterServer.MP4;
using ManiaAPI.MasterServer.TMT;
using ManiaAPI.MasterServer.TMUF;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.Extensions.Hosting.Tests;

public class ServiceRegistrationTests
{
    [Test]
    public async Task AddMasterServerTMUF_RegistersInterface()
    {
        var services = new ServiceCollection();
        services.AddMasterServerTMUF();
        using var provider = services.BuildServiceProvider();

        // Resolved by Type, as the interface's static abstract members keep it from being a generic type argument
        await Assert.That(provider.GetRequiredService(typeof(IMasterServerTMUF))).IsTypeOf<MasterServerTMUF>();
    }

    [Test]
    public async Task AddMasterServerMP4_RegistersInterfaces()
    {
        var services = new ServiceCollection();
        services.AddMasterServerMP4();
        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<IInitServerMP4>()).IsTypeOf<InitServerMP4>();
        await Assert.That(provider.GetRequiredService<IMasterServerMP4>()).IsTypeOf<MasterServerMP4>();
    }

    [Test]
    public async Task AddMasterServerMP3_RegistersInterfaces()
    {
        var services = new ServiceCollection();
        services.AddMasterServerMP3();
        using var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetRequiredService<IInitServerMP3>()).IsTypeOf<InitServerMP3>();
        await Assert.That(provider.GetRequiredService<IMasterServerMP3>()).IsTypeOf<MasterServerMP3>();
    }

    [Test]
    public async Task AddMasterServerTMT_InterfacesAreSameScopedClients()
    {
        var services = new ServiceCollection();
        services.AddMasterServerTMT();
        // The real factory needs the waiting params from the init servers, which would send requests
        services.AddSingleton<IMasterServerTMTFactory, NoPlatformsFactory>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider;

        foreach (var platform in Enum.GetValues<Platform>())
        {
            await Assert.That(scoped.GetRequiredKeyedService<IInitServerTMT>(platform))
                .IsSameReferenceAs(scoped.GetRequiredKeyedService<InitServerTMT>(platform));
        }

        await Assert.That(scoped.GetRequiredService<IAggregatedMasterServerTMT>())
            .IsSameReferenceAs(scoped.GetRequiredService<AggregatedMasterServerTMT>());
    }

    private sealed class NoPlatformsFactory : IMasterServerTMTFactory
    {
        public ImmutableDictionary<Platform, MasterServerResponse<WaitingParams>> WaitingParams { get; } =
            ImmutableDictionary<Platform, MasterServerResponse<WaitingParams>>.Empty;

        public Task RequestWaitingParamsAsync(string? login = null, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public MasterServerTMT CreateClient(Platform platform) => throw new NotSupportedException();

        public ImmutableDictionary<Platform, MasterServerTMT> CreateClients() => ImmutableDictionary<Platform, MasterServerTMT>.Empty;
    }
}
