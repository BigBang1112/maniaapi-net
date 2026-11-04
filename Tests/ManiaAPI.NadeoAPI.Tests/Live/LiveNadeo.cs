namespace ManiaAPI.NadeoAPI.Tests.Live;

/// <summary>
/// Authorizes each service once per test session, so the live tests don't hit the authentication rate limit.
/// </summary>
internal static class LiveNadeo
{
    private static readonly Lazy<Task<NadeoServices>> services = new(() => AuthorizeAsync(new NadeoServices(CreateClient())));
    private static readonly Lazy<Task<NadeoLiveServices>> liveServices = new(() => AuthorizeAsync(new NadeoLiveServices(CreateClient())));
    private static readonly Lazy<Task<NadeoMeetServices>> meetServices = new(() => AuthorizeAsync(new NadeoMeetServices(CreateClient())));

    public static Task<NadeoServices> GetServicesAsync() => services.Value;
    public static Task<NadeoLiveServices> GetLiveServicesAsync() => liveServices.Value;
    public static Task<NadeoMeetServices> GetMeetServicesAsync() => meetServices.Value;

    private static HttpClient CreateClient()
    {
        var http = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false // so a redirect can't drop the Authorization header unnoticed
        });

        http.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NadeoAPI.Tests/1.0");

        return http;
    }

    private static async Task<T> AuthorizeAsync<T>(T api) where T : NadeoAPI
    {
        // Works with a dedicated server account or a service account.
        var login = LiveSecrets.Require("DedicatedServer:Login", "NADEO_API_LOGIN");
        var password = LiveSecrets.Require("DedicatedServer:Password", "NADEO_API_PASSWORD");

        await api.AuthorizeAsync(login, password);

        return api;
    }
}
