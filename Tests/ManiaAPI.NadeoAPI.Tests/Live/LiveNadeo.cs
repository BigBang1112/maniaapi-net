namespace ManiaAPI.NadeoAPI.Tests.Live;

/// <summary>
/// The kind of account a live test authorizes with. Some endpoints behave differently for each.
/// </summary>
public enum NadeoAccount
{
    DedicatedServer,
    ServiceAccount
}

/// <summary>
/// Authorizes each service once per account and test session, so the live tests don't hit the authentication rate limit.
/// </summary>
internal static class LiveNadeo
{
    private static readonly Dictionary<NadeoAccount, Lazy<Task<NadeoServices>>> services = ForEachAccount(
        account => AuthorizeAsync(new NadeoServices(CreateClient()), account));
    private static readonly Dictionary<NadeoAccount, Lazy<Task<NadeoLiveServices>>> liveServices = ForEachAccount(
        account => AuthorizeAsync(new NadeoLiveServices(CreateClient()), account));
    private static readonly Dictionary<NadeoAccount, Lazy<Task<NadeoMeetServices>>> meetServices = ForEachAccount(
        account => AuthorizeAsync(new NadeoMeetServices(CreateClient()), account));

    public static Task<NadeoServices> GetServicesAsync(NadeoAccount account) => services[account].Value;
    public static Task<NadeoLiveServices> GetLiveServicesAsync(NadeoAccount account) => liveServices[account].Value;
    public static Task<NadeoMeetServices> GetMeetServicesAsync(NadeoAccount account) => meetServices[account].Value;

    private static Dictionary<NadeoAccount, Lazy<Task<T>>> ForEachAccount<T>(Func<NadeoAccount, Task<T>> authorize)
    {
        return Enum.GetValues<NadeoAccount>().ToDictionary(account => account, account => new Lazy<Task<T>>(() => authorize(account)));
    }

    private static HttpClient CreateClient()
    {
        var http = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false // so a redirect can't drop the Authorization header unnoticed
        });

        http.DefaultRequestHeaders.UserAgent.ParseAdd("ManiaAPI.NadeoAPI.Tests/1.0");

        return http;
    }

    private static async Task<T> AuthorizeAsync<T>(T api, NadeoAccount account) where T : NadeoAPI
    {
        var (login, password) = account switch
        {
            NadeoAccount.DedicatedServer => (LiveSettings.NadeoAPI.DedicatedServer.Login, LiveSettings.NadeoAPI.DedicatedServer.Password),
            NadeoAccount.ServiceAccount => (LiveSettings.NadeoAPI.ServiceAccount.Login, LiveSettings.NadeoAPI.ServiceAccount.Password),
            _ => throw new ArgumentOutOfRangeException(nameof(account))
        };

        await api.AuthorizeAsync(login, password);

        return api;
    }
}
