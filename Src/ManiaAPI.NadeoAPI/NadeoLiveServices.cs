using ManiaAPI.NadeoAPI.JsonContexts;

namespace ManiaAPI.NadeoAPI;

public interface INadeoLiveServices : INadeoAPI
{
    Task<Maniapub[]> GetActiveManiapubsAsync(CancellationToken cancellationToken = default);
    Task<MapInfo> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<MapInfo[]> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    Task<MapInfo[]> GetMapInfosAsync(params string[] mapUids);
    Task<MedalRecordCollection> GetMapMedalRecordsAsync(string mapUid, string groupId, CancellationToken cancellationToken = default);
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

    public virtual async Task<MapInfo> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/map/{mapUid}", NadeoAPIJsonContext.Default.MapInfo, cancellationToken);
    }

    public virtual async Task<MapInfo[]> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"token/map/get-multiple?mapUidList={string.Join(',', mapUids)}",
            NadeoAPIJsonContext.Default.MapInfoCollection, cancellationToken)).MapList;
    }

    public async Task<MapInfo[]> GetMapInfosAsync(params string[] mapUids)
    {
        return await GetMapInfosAsync(mapUids, cancellationToken: default);
    }

    public virtual async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/Personal_Best/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}",
            NadeoAPIJsonContext.Default.TopLeaderboardCollection, cancellationToken);
    }

    public virtual async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupId, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/{groupId}/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}",
            NadeoAPIJsonContext.Default.TopLeaderboardCollection, cancellationToken);
    }

    public virtual async Task<Maniapub[]> GetActiveManiapubsAsync(CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"token/advertising/display/active", NadeoAPIJsonContext.Default.ManiapubCollection, cancellationToken)).DisplayList;
    }

    public virtual async Task<MedalRecordCollection> GetMapMedalRecordsAsync(string mapUid, string groupId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/{groupId}/map/{mapUid}/medals",
            NadeoAPIJsonContext.Default.MedalRecordCollection, cancellationToken);
    }

    public virtual async Task<MedalRecordCollection> GetMapMedalRecordsAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/Personal_Best/map/{mapUid}/medals",
            NadeoAPIJsonContext.Default.MedalRecordCollection, cancellationToken);
    }
}
