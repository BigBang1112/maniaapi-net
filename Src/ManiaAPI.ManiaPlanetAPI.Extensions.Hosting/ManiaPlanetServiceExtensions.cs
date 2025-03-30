using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

public static class ManiaPlanetServiceExtensions
{
    public static IHttpClientBuilder AddManiaPlanetAPI(this IServiceCollection services)
    {
        services.AddSingleton<ManiaPlanetAPIHandler>();
        return services.AddHttpClient<ManiaPlanetAPI>();
    }
}
