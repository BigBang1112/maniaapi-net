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
    Task<CampaignCollection> GetSeasonalCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<SeasonPlayerRankingCollection> GetPlayerSeasonRankingsAsync(Guid accountId, string groupId, CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetWeeklyCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubMember> GetClubMemberAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default);
    Task<ClubMember> GetClubMemberAsync(int clubId, string diplayName, CancellationToken cancellationToken = default);
    Task<ClubActivityCollection> GetClubActivitiesAsync(int clubId, int length, int offset = 0, bool active = true, CancellationToken cancellationToken = default);
    Task<Club> GetClubAsync(int clubId, CancellationToken cancellationToken = default);
    Task<ClubCampaign> GetClubCampaignAsync(int clubId, int campaignId, CancellationToken cancellationToken = default);
    Task<ClubCampaignCollection> GetClubCampaignsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default);
    Task<ClubCompetitionCollection> GetClubCompetitionsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default);
    Task<ClubMapReviewRoomCollection> GetClubMapReviewRoomsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default);
    Task<ClubMemberCollection> GetClubMembersAsync(int clubId, int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubRoom> GetClubRoomAsync(int clubId, int roomId, CancellationToken cancellationToken = default);
    Task<ClubRoomCollection> GetClubRoomsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default);
    Task<ClubBucketCollection> GetClubBucketsAsync(ClubBucketType type, int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubBucketCollection> GetClubBucketsAsync(string type, int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubBucket> GetClubBucketAsync(int clubId, int bucketId, int length = 1, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubCollection> GetClubsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default);
    Task<ClubPlayerInfo> GetClubPlayerInfoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="length">Number of clubs to request. Max allowed is 250, but response provides <see cref="ClubCollection.ItemCount"/> to allow pagination.</param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubCollection> GetMyClubsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubActivity> EditClubActivityAsync(int clubId, int activityId, ClubActivityEdition edition, CancellationToken cancellationToken = default);
    Task<ClubCampaign> EditClubCampaignAsync(int clubId, int campaignId, ClubCampaignEdition edition, CancellationToken cancellationToken = default);
    Task<ClubActivity> CreateClubFolderAsync(int clubId, string folderName, CancellationToken cancellationToken = default);

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

    public NadeoLiveServices(HttpClient client, NadeoAPIHandler handler, bool automaticallyAuthorize = true) : base(client, handler, automaticallyAuthorize)
    {
    }

    public NadeoLiveServices(bool automaticallyAuthorize = true) : this(new HttpClient(), new NadeoAPIHandler(), automaticallyAuthorize)
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

    public virtual async Task<CampaignCollection> GetSeasonalCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/official?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<SeasonPlayerRankingCollection> GetPlayerSeasonRankingsAsync(Guid accountId, string groupId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/leaderboard/group/{groupId}?accountId={accountId}",
            NadeoAPIJsonContext.Default.SeasonPlayerRankingCollection, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetWeeklyCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/weekly-shorts?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<ClubMember> GetClubMemberAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/member/{accountId}",
            NadeoAPIJsonContext.Default.ClubMember, cancellationToken);
    }

    public virtual async Task<ClubMember> GetClubMemberAsync(int clubId, string diplayName, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/member/{diplayName}/from-login",
            NadeoAPIJsonContext.Default.ClubMember, cancellationToken);
    }

    public virtual async Task<ClubActivityCollection> GetClubActivitiesAsync(int clubId, int length, int offset = 0, bool active = true, int folderId = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/activity?length={length}&offset={offset}&active={active}&folderId={active}",
            NadeoAPIJsonContext.Default.ClubActivityCollection, cancellationToken);
    }

    public virtual async Task<Club> GetClubAsync(int clubId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}", NadeoAPIJsonContext.Default.Club, cancellationToken);
    }

    public virtual async Task<ClubCampaign> GetClubCampaignAsync(int clubId, int campaignId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/campaign/{campaignId}",
            NadeoAPIJsonContext.Default.ClubCampaign, cancellationToken);
    }

    public virtual async Task<ClubCampaignCollection> GetClubCampaignsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/campaign?length={length}&offset={offset}{(name is null ? "" : $"&name={HttpUtility.UrlEncode(name)}")}",
            NadeoAPIJsonContext.Default.ClubCampaignCollection, cancellationToken);
    }

    public virtual async Task<ClubCompetitionCollection> GetClubCompetitionsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/competition?length={length}&offset={offset}{(name is null ? "" : $"&name={HttpUtility.UrlEncode(name)}")}",
            NadeoAPIJsonContext.Default.ClubCompetitionCollection, cancellationToken);
    }

    public virtual async Task<ClubMapReviewRoomCollection> GetClubMapReviewRoomsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/map-review?length={length}&offset={offset}{(name is null ? "" : $"&name={HttpUtility.UrlEncode(name)}")}",
            NadeoAPIJsonContext.Default.ClubMapReviewRoomCollection, cancellationToken);
    }

    public virtual async Task<ClubMemberCollection> GetClubMembersAsync(int clubId, int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/member?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubMemberCollection, cancellationToken);
    }

    public virtual async Task<ClubRoom> GetClubRoomAsync(int clubId, int roomId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/room/{roomId}",
            NadeoAPIJsonContext.Default.ClubRoom, cancellationToken);
    }

    public virtual async Task<ClubRoomCollection> GetClubRoomsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/room?length={length}&offset={offset}{(name is null ? "" : $"&name={HttpUtility.UrlEncode(name)}")}",
            NadeoAPIJsonContext.Default.ClubRoomCollection, cancellationToken);
    }

    public virtual async Task<ClubBucketCollection> GetClubBucketsAsync(ClubBucketType type, int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        var typeStr = type switch
        {
            ClubBucketType.MapUpload => "map-upload",
            ClubBucketType.SkinUpload => "skin-upload",
            ClubBucketType.ItemUpload => "item-upload",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        return await GetClubBucketsAsync(typeStr, length, offset, cancellationToken);
    }

    public virtual async Task<ClubBucketCollection> GetClubBucketsAsync(string type, int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/bucket/{type}/all?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubBucketCollection, cancellationToken);
    }

    public virtual async Task<ClubBucket> GetClubBucketAsync(int clubId, int bucketId, int length = 1, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/bucket/{bucketId}?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubBucket, cancellationToken);
    }

    public virtual async Task<ClubCollection> GetClubsAsync(int length, int offset = 0, string? name = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club?length={length}&offset={offset}{(name is null ? "" : $"&name={HttpUtility.UrlEncode(name)}")}",
            NadeoAPIJsonContext.Default.ClubCollection, cancellationToken);
    }

    public virtual async Task<ClubPlayerInfo> GetClubPlayerInfoAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/player/info", NadeoAPIJsonContext.Default.ClubPlayerInfo, cancellationToken);
    }

    public virtual async Task<ClubCollection> GetMyClubsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/mine?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubCollection, cancellationToken);
    }

    public virtual async Task<ClubActivity> EditClubActivityAsync(int clubId, int activityId, ClubActivityEdition edition, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(edition, NadeoAPIJsonContext.Default.ClubActivityEdition);
        return await PostJsonAsync($"token/club/{clubId}/activity/{activityId}/edit",
            jsonContent, NadeoAPIJsonContext.Default.ClubActivity, cancellationToken);
    }

    public virtual async Task<ClubCampaign> EditClubCampaignAsync(int clubId, int campaignId, ClubCampaignEdition edition, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(edition, NadeoAPIJsonContext.Default.ClubCampaignEdition);
        return await PostJsonAsync($"token/club/{clubId}/campaign/{campaignId}/edit",
            jsonContent, NadeoAPIJsonContext.Default.ClubCampaign, cancellationToken);
    }

    public virtual async Task<ClubActivity> CreateClubFolderAsync(int clubId, string folderName, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(new ClubFolder(folderName, FolderId: 0), NadeoAPIJsonContext.Default.ClubFolder);
        return await PostJsonAsync($"token/club/{clubId}/folder/create", jsonContent, NadeoAPIJsonContext.Default.ClubActivity, cancellationToken: cancellationToken);
    }

    public virtual async Task<string> JoinDailyChannelAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "token/channel/daily/join", cancellationToken: cancellationToken);
        return (await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.DailyChannelJoin, cancellationToken))?.JoinLink ?? throw new Exception("This shouldn't be null.");
    }
}
