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
            PendingCredentials = o.Credentials
        });
        services.AddKeyedSingleton(nameof(NadeoLiveServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials
        });
        services.AddKeyedSingleton(nameof(NadeoMeetServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials
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

        services.AddTransient<INadeoServices>(provider => provider.GetRequiredService<NadeoServices>());
        services.AddTransient<INadeoLiveServices>(provider => provider.GetRequiredService<NadeoLiveServices>());
        services.AddTransient<INadeoMeetServices>(provider => provider.GetRequiredService<NadeoMeetServices>());

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
            PendingCredentials = o.Credentials
        });

        var httpBuilder = services.AddHttpClient(nameof(NadeoServices));

        services.AddTransient(provider => new NadeoServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoServices)), o.UserAgent));
        services.AddTransient<INadeoServices>(provider => provider.GetRequiredService<NadeoServices>());

        services.AddHostedService<NadeoAPIRefreshBackgroundService>();

        return httpBuilder;
    }

    public static IHttpClientBuilder AddNadeoLiveServices(this IServiceCollection services, Action<NadeoAPIOptions> options)
    {
        var o = new NadeoAPIOptions();
        options(o);

        services.AddKeyedSingleton(nameof(NadeoLiveServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials
        });

        var httpBuilder = services.AddHttpClient(nameof(NadeoLiveServices));

        services.AddTransient(provider => new NadeoLiveServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoLiveServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoLiveServices)), o.UserAgent));
        services.AddTransient<INadeoLiveServices>(provider => provider.GetRequiredService<NadeoLiveServices>());

        services.AddHostedService<NadeoAPIRefreshBackgroundService>();

        return httpBuilder;
    }

    public static IHttpClientBuilder AddNadeoMeetServices(this IServiceCollection services, Action<NadeoAPIOptions> options)
    {
        var o = new NadeoAPIOptions();
        options(o);

        services.AddKeyedSingleton(nameof(NadeoMeetServices), new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials
        });

        var httpBuilder = services.AddHttpClient(nameof(NadeoMeetServices));

        services.AddTransient(provider => new NadeoMeetServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoMeetServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoMeetServices)), o.UserAgent));
        services.AddTransient<INadeoMeetServices>(provider => provider.GetRequiredService<NadeoMeetServices>());

        services.AddHostedService<NadeoAPIRefreshBackgroundService>();

        return httpBuilder;
    }
}
