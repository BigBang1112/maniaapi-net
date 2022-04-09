
namespace ManiaAPI.NadeoAPI;

public interface INadeoLiveServices : INadeoAPI
{
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
}
