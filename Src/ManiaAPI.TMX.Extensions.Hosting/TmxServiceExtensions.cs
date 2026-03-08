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
            if (!string.IsNullOrEmpty(options.UserAgent))
            {
                builder.ConfigureHttpClient(client => {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                });
            }
            options.ConfigureHttpClient?.Invoke(builder);
                
            services.AddKeyedScoped(site, (provider, key) => new TMX(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(TMX)}_{key}"), site));
            services.AddScoped(provider => provider.GetRequiredKeyedService<TMX>(site));
        }

        foreach (var site in Enum.GetValues<MxSite>())
        {
            var builder = services.AddHttpClient($"{nameof(MX)}_{site}");
            if (!string.IsNullOrEmpty(options.UserAgent))
            {
                builder.ConfigureHttpClient(client => {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                });
            }
            options.ConfigureHttpClient?.Invoke(builder);

            services.AddKeyedScoped(site, (provider, key) => new MX(
                provider.GetRequiredService<IHttpClientFactory>().CreateClient($"{nameof(MX)}_{key}"), site));
            services.AddScoped(provider => provider.GetRequiredKeyedService<MX>(site));
        }

        services.AddScoped(provider => Enum.GetValues<TmxSite>()
            .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<TMX>(site)));
        services.AddScoped(provider => Enum.GetValues<MxSite>()
            .ToImmutableDictionary(site => site, site => provider.GetRequiredKeyedService<MX>(site)));
    }
}
