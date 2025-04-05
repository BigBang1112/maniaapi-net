using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

public static class TrackmaniaServiceExtensions
{
    public static IHttpClientBuilder AddTrackmaniaAPI(this IServiceCollection services, Action<TrackmaniaAPIOptions> options)
    {
        var o = new TrackmaniaAPIOptions();
        options(o);

        services.AddSingleton(new TrackmaniaAPIHandler
        {
            Credentials = o.Credentials
        });

        return services.AddHttpClient<TrackmaniaAPI>();
    }

    public static IHttpClientBuilder AddTrackmaniaAPI(this IServiceCollection services)
    {
        return AddTrackmaniaAPI(services, _ => { });
    }
}
