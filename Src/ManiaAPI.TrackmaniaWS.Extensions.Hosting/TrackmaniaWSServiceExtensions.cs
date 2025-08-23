using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.TrackmaniaWS.Extensions.Hosting;

public static class TrackmaniaWSServiceExtensions
{
    public static IHttpClientBuilder AddTrackmaniaWS(this IServiceCollection services, TrackmaniaWSOptions options)
    {
        var builder = services.AddHttpClient<TrackmaniaWS>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(TrackmaniaWS.BaseAddress));
        services.AddTransient(provider => new TrackmaniaWS(
            options.Credentials ?? throw new Exception("Credentials are required currently."),
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(TrackmaniaWS))));
        return builder;
    }
}
