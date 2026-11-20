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

        services.AddTransient<IManiaPlanetAPI>(provider => provider.GetRequiredService<ManiaPlanetAPI>());

        return services.AddHttpClient(nameof(ManiaPlanetAPI))
            .AddTypedClient((client, provider) => new ManiaPlanetAPI(
                client, provider.GetRequiredService<ManiaPlanetAPIHandler>(), o.UserAgent));
    }

    public static IHttpClientBuilder AddManiaPlanetAPI(this IServiceCollection services)
    {
        return AddManiaPlanetAPI(services, _ => { });
    }

    public static IHttpClientBuilder AddManiaPlanetIngameAPI(this IServiceCollection services)
    {
        return AddManiaPlanetIngameAPI(services, _ => { });
    }

    public static IHttpClientBuilder AddManiaPlanetIngameAPI(this IServiceCollection services, Action<ManiaPlanetAPIOptions> options)
    {
        var o = new ManiaPlanetAPIOptions();
        options(o);

        services.AddTransient<IManiaPlanetIngameAPI>(provider => provider.GetRequiredService<ManiaPlanetIngameAPI>());

        return services.AddHttpClient(nameof(ManiaPlanetIngameAPI))
            .AddTypedClient(client => new ManiaPlanetIngameAPI(client, o.UserAgent))
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
        return AddManiaPlanetChannelRecordAPI(services, _ => { });
    }

    public static IHttpClientBuilder AddManiaPlanetChannelRecordAPI(this IServiceCollection services, Action<ManiaPlanetAPIOptions> options)
    {
        var o = new ManiaPlanetAPIOptions();
        options(o);

        services.AddTransient<IManiaPlanetChannelRecordAPI>(provider => provider.GetRequiredService<ManiaPlanetChannelRecordAPI>());

        return services.AddHttpClient(nameof(ManiaPlanetChannelRecordAPI))
            .AddTypedClient(client => new ManiaPlanetChannelRecordAPI(client, o.UserAgent))
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
        return AddManiaPlanetChannelFeedbackAPI(services, _ => { });
    }

    public static IHttpClientBuilder AddManiaPlanetChannelFeedbackAPI(this IServiceCollection services, Action<ManiaPlanetAPIOptions> options)
    {
        var o = new ManiaPlanetAPIOptions();
        options(o);

        services.AddTransient<IManiaPlanetChannelFeedbackAPI>(provider => provider.GetRequiredService<ManiaPlanetChannelFeedbackAPI>());

        return services.AddHttpClient(nameof(ManiaPlanetChannelFeedbackAPI))
            .AddTypedClient(client => new ManiaPlanetChannelFeedbackAPI(client, o.UserAgent))
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(ManiaPlanetChannelFeedbackAPI.BaseAddress);
            });
    }
}
