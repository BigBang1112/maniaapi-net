using Microsoft.Extensions.DependencyInjection;

namespace ManiaAPI.NadeoAPI.Extensions.Hosting;

public static class NadeoAPIServiceExtensions
{
    public static void AddNadeoAPI(this IServiceCollection services, Action<NadeoAPIOptions> options)
    {
        var o = new NadeoAPIOptions();
        options(o);

        services.AddSingleton(new NadeoAPIHandler
        {
            PendingCredentials = o.Credentials
        });

        services.AddHttpClient<NadeoServices>();
        services.AddHttpClient<NadeoLiveServices>();
        services.AddHttpClient<NadeoMeetServices>();
    }

    public static void AddNadeoAPI(this IServiceCollection services)
    {
        AddNadeoAPI(services, _ => { });
    }
}
