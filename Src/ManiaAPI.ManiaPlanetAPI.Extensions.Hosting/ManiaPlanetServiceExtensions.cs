using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

public static class ManiaPlanetServiceExtensions
{
    public static IHttpClientBuilder AddManiaPlanetAPI(this IServiceCollection services, Action<ManiaPlanetAPIOptions> options)
    {
        var o = new ManiaPlanetAPIOptions();
        options(o);

        services.AddSingleton(new ManiaPlanetAPIHandler
        {
            Credentials = o.Credentials
        });

        return services.AddHttpClient<ManiaPlanetAPI>();
    }

    public static IHttpClientBuilder AddManiaPlanetAPI(this IServiceCollection services)
    {
        return AddManiaPlanetAPI(services, _ => { });
    }
}
