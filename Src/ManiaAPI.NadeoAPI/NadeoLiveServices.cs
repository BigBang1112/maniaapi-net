using ManiaAPI.NadeoAPI.JsonContexts;

namespace ManiaAPI.NadeoAPI;

public interface INadeoLiveServices : INadeoAPI
{
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupId, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
}

public class NadeoLiveServices : NadeoAPI, INadeoLiveServices
{
    public override string Audience => nameof(NadeoLiveServices);

    public NadeoLiveServices(HttpClient client, bool automaticallyAuthorize = true) : base(client, automaticallyAuthorize)
    {
        client.BaseAddress = new Uri("https://live-services.trackmania.nadeo.live/api/");
    }

    public NadeoLiveServices(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize)
    {
    }

    public virtual async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/Personal_Best/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}",
            TopLeaderboardCollectionJsonContext.Default.TopLeaderboardCollection, cancellationToken);
    }

    public virtual async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupId, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/{groupId}/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}",
            TopLeaderboardCollectionJsonContext.Default.TopLeaderboardCollection, cancellationToken);
    }
}
