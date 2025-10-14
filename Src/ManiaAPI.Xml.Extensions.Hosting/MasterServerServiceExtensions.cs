using ManiaAPI.Xml.MP4;
using ManiaAPI.Xml.TMT;
using ManiaAPI.Xml.TMUF;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Net;

namespace ManiaAPI.Xml.Extensions.Hosting;

public static class MasterServerServiceExtensions
{
    public static IHttpClientBuilder AddMasterServerTMUF(this IServiceCollection services)
    {
        return services.AddHttpClient<MasterServerTMUF>(client => client.BaseAddress = new Uri(MasterServerTMUF.DefaultAddress));
    }

    public static IHttpClientBuilder AddInitServerMP4(this IServiceCollection services)
    {
        return services.AddHttpClient<InitServerMP4>(client => client.BaseAddress = new Uri(InitServerMP4.DefaultAddress));
    }

    public static void AddMasterServerMP4(this IServiceCollection services)
    {
        services.AddHttpClient<MasterServerMP4>(client => client.BaseAddress = new Uri(MasterServerMP4.DefaultAddress))
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                AutomaticDecompression = DecompressionMethods.GZip
            });
        services.AddSingleton(provider => new MasterServerMP4(provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(MasterServerMP4))));
    }

    public static void AddMasterServerTMT(this IServiceCollection services)
    {
        foreach (var platform in Enum.GetValues<Platform>())
        {
            services.AddHttpClient($"{nameof(InitServerTMT)}_{platform}", client => client.BaseAddress = new Uri(InitServerTMT.GetDefaultAddress(platform)));
            services.AddHttpClient($"{nameof(MasterServerTMT)}_{platform}")
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                    AutomaticDecompression = DecompressionMethods.GZip
                });

            services.AddKeyedScoped(platform, (provider, key) => new InitServerTMT(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(InitServerTMT)}_{key}")));

            services.AddKeyedSingleton(platform, (provider, key) => new MasterServerTMT(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(MasterServerTMT)}_{key}")));
            services.AddSingleton(provider => provider.GetRequiredKeyedService<MasterServerTMT>(platform));
        }

        services.AddSingleton(provider => Enum.GetValues<Platform>()
            .ToImmutableDictionary(platform => platform, platform => provider.GetRequiredKeyedService<MasterServerTMT>(platform)));

        services.AddScoped(provider => new AggregatedMasterServerTMT(
            provider.GetRequiredService<ImmutableDictionary<Platform, MasterServerTMT>>()));
    }
}
