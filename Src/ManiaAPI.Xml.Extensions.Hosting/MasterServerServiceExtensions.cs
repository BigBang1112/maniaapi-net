using ManiaAPI.Xml.MP4;
using ManiaAPI.Xml.TMT;
using ManiaAPI.Xml.TMUF;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Net;

namespace ManiaAPI.Xml.Extensions.Hosting;

/// <summary>
/// Extension methods to register master server related services.
/// </summary>
public static class MasterServerServiceExtensions
{
    /// <summary>
    /// Adds the <see cref="MasterServerTMUF"/> client to the service collection, with its <see cref="HttpClient"/> configured to use the default TMUF master server, but it can be changed with the builder.
    /// </summary>
    public static IHttpClientBuilder AddMasterServerTMUF(this IServiceCollection services)
    {
        return services.AddHttpClient<MasterServerTMUF>(client => client.BaseAddress = new Uri(MasterServerTMUF.DefaultAddress));
    }

    /// <summary>
    /// Adds the <see cref="InitServerMP4"/> and <see cref="MasterServerMP4"/> clients to the service collection, along with the <see cref="IMasterServerMP4Factory"/> to create <see cref="MasterServerMP4"/> clients.
    /// </summary>
    public static void AddMasterServerMP4(this IServiceCollection services,
        Action<IHttpClientBuilder>? configureInitServer = null,
        Action<IHttpClientBuilder>? configureMasterServer = null)
    {
        var httpInitServer = services.AddHttpClient<InitServerMP4>(client => client.BaseAddress = new Uri(InitServerMP4.DefaultAddress));
        var httpMasterServer = services.AddHttpClient<MasterServerMP4>()
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                AutomaticDecompression = DecompressionMethods.GZip
            });
        services.AddTransient(provider => new MasterServerMP4(new Uri(MasterServerMP4.DefaultAddress),
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(MasterServerMP4))));

        if (configureInitServer is not null) configureInitServer(httpInitServer);
        if (configureMasterServer is not null) configureMasterServer(httpMasterServer);

        services.AddSingleton<IMasterServerMP4Factory, MasterServerMP4Factory>();
    }

    /// <summary>
    /// Adds the init and master server clients, along with the <see cref="AggregatedMasterServerTMT"/> and <see cref="IMasterServerTMTFactory"/> to have control of each platform.
    /// </summary>
    public static void AddMasterServerTMT(this IServiceCollection services,
        Action<IHttpClientBuilder>? configureInitServer = null,
        Action<IHttpClientBuilder>? configureMasterServer = null)
    {
        foreach (var platform in Enum.GetValues<Platform>())
        {
            var httpInitServer = services.AddHttpClient($"{nameof(InitServerTMT)}_{platform}", client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(platform)));
            var httpMasterServer = services.AddHttpClient($"{nameof(MasterServerTMT)}_{platform}")
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                    AutomaticDecompression = DecompressionMethods.GZip
                });

            if (configureInitServer is not null) configureInitServer(httpInitServer);
            if (configureMasterServer is not null) configureMasterServer(httpMasterServer);

            services.AddKeyedScoped(platform, (provider, key) => new InitServerTMT(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(InitServerTMT)}_{key}")));
        }

        services.AddScoped(provider => Enum.GetValues<Platform>()
            .ToImmutableDictionary(platform => platform, platform => provider.GetRequiredKeyedService<InitServerTMT>(platform)));

        services.AddSingleton<IMasterServerTMTFactory, MasterServerTMTFactory>();

        services.AddScoped(provider => new AggregatedMasterServerTMT(
            provider.GetRequiredService<IMasterServerTMTFactory>().CreateClients()));
    }
}
