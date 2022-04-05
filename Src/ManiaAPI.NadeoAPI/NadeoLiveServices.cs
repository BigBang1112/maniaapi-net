namespace ManiaAPI.NadeoAPI;

public class NadeoLiveServices : NadeoAPI
{
    public NadeoLiveServices() : base("https://live-services.trackmania.nadeo.live/api/")
    {
        
    }

    public async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<TopLeaderboardCollection>($"token/leaderboard/group/Personal_Best/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}", cancellationToken);
    }
}
