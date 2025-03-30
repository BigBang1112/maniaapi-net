using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

public static class TrackmaniaServiceExtensions
{
    public static IHttpClientBuilder AddTrackmaniaAPI(this IServiceCollection services)
    {
        services.AddSingleton<TrackmaniaAPIHandler>();
        return services.AddHttpClient<TrackmaniaAPI>();
    }
}
