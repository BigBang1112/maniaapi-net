using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.TMX.Extensions.Hosting;

public static class TmxServiceExtensions
{
    public static void AddTMX(this IServiceCollection services)
    {
        foreach (var site in Enum.GetValues<TmxSite>())
        {
            services.AddHttpClient($"{nameof(TMX)}_{site}");

            services.AddKeyedScoped(site, (provider, key) => new TMX(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(TMX)}_{key}"), site));
            services.AddScoped(provider => provider.GetRequiredKeyedService<TMX>(site));
        }

        services.AddScoped(provider => Enum.GetValues<TmxSite>()
            .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<TMX>(site)));
    }
}
