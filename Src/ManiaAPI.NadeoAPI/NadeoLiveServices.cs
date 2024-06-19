using ManiaAPI.NadeoAPI.JsonContexts;
using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Web;

namespace ManiaAPI.NadeoAPI;

public interface INadeoLiveServices : INadeoAPI
{
    Task<ImmutableArray<Maniapub>> GetActiveManiapubsAsync(CancellationToken cancellationToken = default);
    Task<MapInfoLive> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<ImmutableArray<MapInfoLive>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    Task<ImmutableArray<MapInfoLive>> GetMapInfosAsync(params string[] mapUids);
    Task<MedalRecordCollection> GetMapMedalRecordsAsync(string mapUid, string groupId, CancellationToken cancellationToken = default);
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupId, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
    Task<TrackOfTheDayCollection> GetTrackOfTheDaysAsync(int length, int offset = 0, bool royal = false, CancellationToken cancellationToken = default);
    Task<TrackOfTheDayInfo> GetTrackOfTheDayInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<SeasonPlayerRankingCollection> GetPlayerSeasonRankingsAsync(Guid accountId, string groupId, CancellationToken cancellationToken = default);
    Task<ClubCampaignCollection> GetClubCampaignsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests the daily channel join link. It can vary based on server occupancy.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> with a string containing a join link.</returns>
    Task<string> JoinDailyChannelAsync(CancellationToken cancellationToken = default);
}

public class NadeoLiveServices : NadeoAPI, INadeoLiveServices
{
    public override string Audience => nameof(NadeoLiveServices);
    public override string BaseAddress => "https://live-services.trackmania.nadeo.live/api";

    public NadeoLiveServices(HttpClient client, bool automaticallyAuthorize = true) : base(client, automaticallyAuthorize)
    {
    }

    public NadeoLiveServices(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize)
    {
    }

    public virtual async Task<MapInfoLive> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/map/{mapUid}", NadeoAPIJsonContext.Default.MapInfoLive, cancellationToken);
    }

    public virtual async Task<ImmutableArray<MapInfoLive>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"token/map/get-multiple?mapUidList={string.Join(',', mapUids)}",
            NadeoAPIJsonContext.Default.MapInfoLiveCollection, cancellationToken)).MapList;
    }

    public async Task<ImmutableArray<MapInfoLive>> GetMapInfosAsync(params string[] mapUids)
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

    public virtual async Task<ImmutableArray<Maniapub>> GetActiveManiapubsAsync(CancellationToken cancellationToken = default)
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

    public virtual async Task<TrackOfTheDayCollection> GetTrackOfTheDaysAsync(int length, int offset = 0, bool royal = false, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/campaign/month?length={length}&offset={offset}{(royal ? "&royal=true" : "")}",
            NadeoAPIJsonContext.Default.TrackOfTheDayCollection, cancellationToken);
    }

    public virtual async Task<TrackOfTheDayInfo> GetTrackOfTheDayInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/map/{mapUid}", NadeoAPIJsonContext.Default.TrackOfTheDayInfo, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/campaign/official?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<SeasonPlayerRankingCollection> GetPlayerSeasonRankingsAsync(Guid accountId, string groupId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/{groupId}?accountId={accountId}",
            NadeoAPIJsonContext.Default.SeasonPlayerRankingCollection, cancellationToken);
    }

    public virtual async Task<string> JoinDailyChannelAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "token/channel/daily/join", cancellationToken: cancellationToken);
        return (await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.DailyChannelJoin, cancellationToken))?.JoinLink ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<ClubCampaignCollection> GetClubCampaignsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/campaign?offset={offset}&length={length}{(name is null ? "" : $"&name={HttpUtility.UrlEncode(name)}")}",
            NadeoAPIJsonContext.Default.ClubCampaignCollection, cancellationToken);
    }
}
