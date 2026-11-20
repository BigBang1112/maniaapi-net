using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace ManiaAPI.TMX.Extensions.Hosting;

public static class TmxServiceExtensions
{
    public static void AddTMX(this IServiceCollection services, Action<TmxOptions>? configureOptions = null)
    {
        var options = new TmxOptions();
        configureOptions?.Invoke(options);

        foreach (var site in Enum.GetValues<TmxSite>())
        {
            var builder = services.AddHttpClient($"{nameof(TMX)}_{site}");
            options.ConfigureHttpClient?.Invoke(builder);
                
            services.AddKeyedScoped(site, (provider, key) => new TMX(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(TMX)}_{key}"), site, options.UserAgent));
            services.AddScoped(provider => provider.GetRequiredKeyedService<TMX>(site));

            services.AddKeyedScoped<ITMX>(site, (provider, key) => provider.GetRequiredKeyedService<TMX>(key));
            services.AddScoped<ITMX>(provider => provider.GetRequiredKeyedService<TMX>(site));
        }

        foreach (var site in Enum.GetValues<MxSite>())
        {
            var builder = services.AddHttpClient($"{nameof(MX)}_{site}");
            options.ConfigureHttpClient?.Invoke(builder);

            services.AddKeyedScoped(site, (provider, key) => new MX(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(MX)}_{key}"), site, options.UserAgent));
            services.AddScoped(provider => provider.GetRequiredKeyedService<MX>(site));

            services.AddKeyedScoped<IMX>(site, (provider, key) => provider.GetRequiredKeyedService<MX>(key));
            services.AddScoped<IMX>(provider => provider.GetRequiredKeyedService<MX>(site));
        }

        var ixBuilder = services.AddHttpClient(nameof(IX));
        options.ConfigureHttpClient?.Invoke(ixBuilder);

        services.AddScoped(provider => new IX(provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(IX)), options.UserAgent));
        services.AddScoped<IIX>(provider => provider.GetRequiredService<IX>());

        services.AddScoped(provider => Enum.GetValues<TmxSite>()
            .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<TMX>(site)));
        services.AddScoped(provider => Enum.GetValues<MxSite>()
            .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<MX>(site)));
    }
}
