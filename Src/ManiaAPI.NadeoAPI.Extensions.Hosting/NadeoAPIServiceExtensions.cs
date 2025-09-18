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

        var httpNadeoServices = services.AddHttpClient<NadeoServices>();
        var httpNadeoLiveServices = services.AddHttpClient<NadeoLiveServices>();
        var httpNadeoMeetServices = services.AddHttpClient<NadeoMeetServices>();

        if (configureNadeoServices is not null) configureNadeoServices(httpNadeoServices);
        if (configureNadeoLiveServices is not null) configureNadeoLiveServices(httpNadeoLiveServices);
        if (configureNadeoMeetServices is not null) configureNadeoMeetServices(httpNadeoMeetServices);

        services.AddTransient(provider => new NadeoServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoServices))));
        services.AddTransient(provider => new NadeoLiveServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoLiveServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoLiveServices))));
        services.AddTransient(provider => new NadeoMeetServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoMeetServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoMeetServices))));
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

        var httpBuilder = services.AddHttpClient<NadeoServices>();

        services.AddTransient(provider => new NadeoServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoServices))));

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

        var httpBuilder = services.AddHttpClient<NadeoLiveServices>();

        services.AddTransient(provider => new NadeoLiveServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoLiveServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoLiveServices))));

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

        var httpBuilder = services.AddHttpClient<NadeoMeetServices>();

        services.AddTransient(provider => new NadeoMeetServices(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(NadeoMeetServices)),
            provider.GetRequiredKeyedService<NadeoAPIHandler>(nameof(NadeoMeetServices))));

        return httpBuilder;
    }
}
