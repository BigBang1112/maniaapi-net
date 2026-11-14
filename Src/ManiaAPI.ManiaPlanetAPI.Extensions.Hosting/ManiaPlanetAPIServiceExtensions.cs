using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.ManiaPlanetAPI.Extensions.Hosting;

public static class ManiaPlanetAPIServiceExtensions
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

    public static IHttpClientBuilder AddManiaPlanetIngameAPI(this IServiceCollection services)
    {
        return services.AddHttpClient<ManiaPlanetIngameAPI>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(ManiaPlanetIngameAPI.BaseAddress);
            });
    }

    /// <summary>
    /// Adds the ManiaPlanet Channel Record API client (channel map leaderboards), which needs no authorization.
    /// </summary>
    public static IHttpClientBuilder AddManiaPlanetChannelRecordAPI(this IServiceCollection services)
    {
        return services.AddHttpClient<ManiaPlanetChannelRecordAPI>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(ManiaPlanetChannelRecordAPI.BaseAddress);
            });
    }

    /// <summary>
    /// Adds the ManiaPlanet Channel Feedback API client (votes and ratings of channel maps), which needs no authorization.
    /// </summary>
    public static IHttpClientBuilder AddManiaPlanetChannelFeedbackAPI(this IServiceCollection services)
    {
        return services.AddHttpClient<ManiaPlanetChannelFeedbackAPI>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(ManiaPlanetChannelFeedbackAPI.BaseAddress);
            });
    }
}
