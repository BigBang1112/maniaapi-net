using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting;

public static class NadeoAPIServiceExtensions
{
    public static void AddNadeoAPI(
        this IServiceCollection services, 
        Action<NadeoAPIOptions> options, 
        Action<IHttpClientBuilder>? configureNadeoServices = null,
        Action<IHttpClientBuilder>? configureNadeoLiveServices = null,
        Action<IHttpClientBuilder>? configureNadeoMeetServices = null)
    {
        var o = new NadeoAPIOptions();
        options(o);

        services.AddKeyedSingleton(nameof(NadeoServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials,
            SaveCredentials = false
        });
        services.AddKeyedSingleton(nameof(NadeoLiveServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials,
            SaveCredentials = false
        });
        services.AddKeyedSingleton(nameof(NadeoMeetServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials,
            SaveCredentials = false
        });

        var httpNadeoServices = services.AddHttpClient(nameof(NadeoServices));
        var httpNadeoLiveServices = services.AddHttpClient(nameof(NadeoLiveServices));
        var httpNadeoMeetServices = services.AddHttpClient(nameof(NadeoMeetServices));

        if (configureNadeoServices is not null) configureNadeoServices(httpNadeoServices);
        if (configureNadeoLiveServices is not null) configureNadeoLiveServices(httpNadeoLiveServices);
        if (configureNadeoMeetServices is not null) configureNadeoMeetServices(httpNadeoMeetServices);

        services.AddTransient(provider => new NadeoServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoServices)), o.UserAgent));
        services.AddTransient(provider => new NadeoLiveServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoLiveServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoLiveServices)), o.UserAgent));
        services.AddTransient(provider => new NadeoMeetServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoMeetServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoMeetServices)), o.UserAgent));

        services.AddHostedService<NadeoAPIRefreshBackgroundService>();
    }

    public static void AddNadeoAPI(
        this IServiceCollection services,
        Action<IHttpClientBuilder>? configureNadeoServices = null,
        Action<IHttpClientBuilder>? configureNadeoLiveServices = null,
        Action<IHttpClientBuilder>? configureNadeoMeetServices = null)
    {
        AddNadeoAPI(services, _ => { }, configureNadeoServices, configureNadeoLiveServices, configureNadeoMeetServices);
    }

    public static IHttpClientBuilder AddNadeoServices(this IServiceCollection services, Action<NadeoAPIOptions> options)
    {
        var o = new NadeoAPIOptions();
        options(o);

        services.AddKeyedSingleton(nameof(NadeoServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials,
            SaveCredentials = false
        });

        var httpBuilder = services.AddHttpClient(nameof(NadeoServices));

        services.AddTransient(provider => new NadeoServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoServices)), o.UserAgent));

        services.AddHostedService<NadeoAPIRefreshBackgroundService>();

        return httpBuilder;
    }

    public static IHttpClientBuilder AddNadeoLiveServices(this IServiceCollection services, Action<NadeoAPIOptions> options)
    {
        var o = new NadeoAPIOptions();
        options(o);

        services.AddKeyedSingleton(nameof(NadeoLiveServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials,
            SaveCredentials = false
        });

        var httpBuilder = services.AddHttpClient(nameof(NadeoLiveServices));

        services.AddTransient(provider => new NadeoLiveServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoLiveServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoLiveServices)), o.UserAgent));

        services.AddHostedService<NadeoAPIRefreshBackgroundService>();

        return httpBuilder;
    }

    public static IHttpClientBuilder AddNadeoMeetServices(this IServiceCollection services, Action<NadeoAPIOptions> options)
    {
        var o = new NadeoAPIOptions();
        options(o);

        services.AddKeyedSingleton(nameof(NadeoMeetServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials,
            SaveCredentials = false
        });

        var httpBuilder = services.AddHttpClient(nameof(NadeoMeetServices));

        services.AddTransient(provider => new NadeoMeetServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoMeetServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoMeetServices)), o.UserAgent));

        services.AddHostedService<NadeoAPIRefreshBackgroundService>();

        return httpBuilder;
    }
}
