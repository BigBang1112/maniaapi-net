using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaAPI.Extensions.Hosting;

public static class TrackmaniaAPIServiceExtensions
{
    public static IHttpClientBuilder AddTrackmaniaAPI(this IServiceCollection services, Action<TrackmaniaAPIOptions> options)
    {
        var o = new TrackmaniaAPIOptions();
        options(o);

        services.AddSingleton(new TrackmaniaAPIHandler
        {
            Credentials = o.Credentials
        });

        return services.AddHttpClient(nameof(TrackmaniaAPI))
            .AddTypedClient((client, provider) => new TrackmaniaAPI(
                client, provider.GetRequiredService<TrackmaniaAPIHandler>(), o.UserAgent));
    }

    public static IHttpClientBuilder AddTrackmaniaAPI(this IServiceCollection services)
    {
        return AddTrackmaniaAPI(services, _ => { });
    }
}
