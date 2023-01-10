namespace ManiaAPI.NadeoAPI;

public interface INadeoLiveServices : INadeoAPI
{
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupId, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
}

public class NadeoLiveServices : NadeoAPI, INadeoLiveServices
{
    public NadeoLiveServices(bool automaticallyAuthorize = true) : base("https://live-services.trackmania.nadeo.live/api/", automaticallyAuthorize)
    {
        
    }

    public async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<TopLeaderboardCollection>($"token/leaderboard/group/Personal_Best/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}", cancellationToken);
    }

    public async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupId, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<TopLeaderboardCollection>($"token/leaderboard/group/{groupId}/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}", cancellationToken);
    }
}
