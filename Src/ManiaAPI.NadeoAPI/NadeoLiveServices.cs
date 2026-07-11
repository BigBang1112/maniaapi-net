using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Web;

namespace ManiaAPI.NadeoAPI;

public interface INadeoLiveServices : INadeoAPI
{
    Task<ImmutableList<Maniapub>> GetActiveManiapubsAsync(CancellationToken cancellationToken = default);
    Task<MapInfoLive?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapInfoLive>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapInfoLive>> GetMapInfosAsync(params string[] mapUids);
    Task<MedalRecordCollection> GetMapMedalRecordsAsync(string mapUid, string groupUid, CancellationToken cancellationToken = default);
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
    Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default);
    Task<ImmutableList<Position>> GetLeaderboardPositionByTimeAsync(string mapUid, string groupUid, int score, CancellationToken cancellationToken = default);
    Task<ImmutableList<Position>> GetLeaderboardPositionsByTimeAsync(IEnumerable<string> mapUids, IEnumerable<string> groupUids, IEnumerable<int> scores, CancellationToken cancellationToken = default);
    Task<ImmutableList<Position>> GetLeaderboardPositionByTimeAsync(string mapUid, int score, CancellationToken cancellationToken = default);
    Task<ImmutableList<Position>> GetLeaderboardPositionsByTimeAsync(IEnumerable<string> mapUids, IEnumerable<int> scores, CancellationToken cancellationToken = default);
    Task<TrackOfTheDayCollection> GetTrackOfTheDaysAsync(int length, int offset = 0, bool royal = false, CancellationToken cancellationToken = default);
    Task<TrackOfTheDayInfo> GetTrackOfTheDayInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetSeasonalCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<SeasonPlayerRankingCollection> GetPlayerSeasonRankingsAsync(Guid accountId, string groupUid, CancellationToken cancellationToken = default);
    [Obsolete("Use GetWeeklyShortCampaignsAsync instead.")]
    Task<CampaignCollection> GetWeeklyCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetWeeklyShortCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetWeeklyGrandCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubMember> GetClubMemberAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default);
    Task<ClubMember> GetClubMemberAsync(int clubId, string displayName, CancellationToken cancellationToken = default);
    Task<ClubActivityCollection> GetClubActivitiesAsync(int clubId, int length, int offset = 0, bool active = true, int folderId = 0, CancellationToken cancellationToken = default);
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
    Task DeleteClubActivityAsync(int clubId, int activityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests the daily channel join link. It can vary based on server occupancy.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> with a string containing a join link.</returns>
    Task<string> JoinDailyChannelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// This request returns empty list if authenticated through dedicated server.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PlayerServerAccountCollection> GetDedicatedServerAccountsAsync(CancellationToken cancellationToken = default);
    Task<CampaignCollection> GetCupOfTheWeekCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// The <paramref name="groupUid"/> "Personal_Best" cannot be used for this endpoint because it requires a group that refers to a campaign or season.
    /// </summary>
    /// <param name="groupUid"></param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="onlyWorld"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CampaignRankingCollection> GetCampaignLeaderboardAsync(string groupUid, int length = 5, int offset = 0, bool? onlyWorld = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// The <paramref name="groupUid"/> "Personal_Best" cannot be used for this endpoint because it requires a group that refers to a campaign or season.
    /// </summary>
    /// <param name="groupUid"></param>
    /// <param name="clubId"></param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubCampaignTopCollection> GetClubCampaignLeaderboardAsync(string groupUid, int clubId, int length = 5, int offset = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request returns empty model if authenticated through dedicated server. The <paramref name="groupUid"/> "Personal_Best" cannot be used for this endpoint because it requires a group that refers to a campaign or season.
    /// </summary>
    /// <param name="groupUid"></param>
    /// <param name="clubId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubCampaignRanking> GetPlayerClubCampaignRankingAsync(string groupUid, int clubId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="groupUid"></param>
    /// <param name="mapUid"></param>
    /// <param name="clubId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubMapRecord> GetPlayerClubMapRecordAsync(string groupUid, string mapUid, int clubId, CancellationToken cancellationToken = default);
    Task<ClubMapRecord> GetPlayerClubMapRecordAsync(string mapUid, int clubId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the currently authenticated user's records on multiple maps. A maximum of 50 records may be requested.
    /// </summary>
    /// <param name="mapUids"></param>
    /// <param name="groupUids"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ImmutableList<Position>> GetPlayerRecordsAsync(IEnumerable<string> mapUids, IEnumerable<string> groupUids, CancellationToken cancellationToken = default);
    Task<ImmutableList<Position>> GetPlayerRecordsAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    /// <summary>
    /// The <paramref name="groupUid"/> "Personal_Best" can be used to get the global leaderboard.
    /// </summary>
    /// <param name="groupUid"></param>
    /// <param name="mapUid"></param>
    /// <param name="lower">The amount of records to return that are lower than the requested score. Max 1.</param>
    /// <param name="upper">The amount of records to return that are higher than the requested score. Max 1.</param>
    /// <param name="score"></param>
    /// <param name="onlyWorld"></param>
    /// <param name="zoneId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TopLeaderboardCollection> GetSurroundingRecordsAsync(string groupUid, string mapUid, int lower, int upper, int? score = null, bool? onlyWorld = null, string? zoneId = null, CancellationToken cancellationToken = default);
    Task<ClubTopLeaderboard> GetClubTopLeaderboardAsync(string groupUid, string mapUid, int clubId, int length = 5, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubTopLeaderboard> GetClubTopLeaderboardAsync(string mapUid, int clubId, int length = 5, int offset = 0, CancellationToken cancellationToken = default);
    Task<TrophyRankingCollection> GetPlayerTrophyRankingsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<TrophyRankingCollection> GetPlayerTrophyRankingsAsync(params Guid[] accountIds);
    /// <summary>
    /// This request returns empty model if authenticated through dedicated server.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TrophyPlayerRanking> GetOwnTrophyRankingAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account, but does not error out on dedicated servers (silently fails).
    /// </summary>
    /// <param name="mapUid"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account, but does not error out on dedicated servers (silently fails).
    /// </summary>
    /// <param name="mapUid"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RemoveFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request returns empty list if authenticated through dedicated server.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="sort">Either "date" or "name".</param>
    /// <param name="order">Either "asc" or "desc".</param>
    /// <param name="mapTypeList"></param>
    /// <param name="playable"></param>
    /// <param name="onlyMine"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MapInfoLiveCollection> GetFavoriteMapsAsync(int length, int offset = 0, string sort = "date", string order = "desc", string? mapTypeList = null, bool? playable = null, bool? onlyMine = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request returns empty list if authenticated through dedicated server.
    /// </summary>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MapInfoLiveCollection> GetUploadedMapsAsync(int length, int offset = 0, CancellationToken cancellationToken = default);

    Task<ClubMapReviewRoom> GetClubMapReviewActivityAsync(int clubId, int activityId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account. If the currently authenticated account has already joined the club, this returns the existing membership.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubMember> JoinClubAsync(int clubId, CancellationToken cancellationToken = default);
    Task<ClubNews> GetClubNewsAsync(int clubId, int activityId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account. If the club is already pinned, calling this again unpins it.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubTagInfo> PinClubAsync(int clubId, CancellationToken cancellationToken = default);
    /// <summary>
    /// If the requested room is inactive, calling this starts it. <see cref="ClubRoomJoinLink.JoinLink"/> will be empty while the room isn't ready yet, which is also reflected by <see cref="ClubRoomJoinLink.Starting"/>.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="activityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubRoomJoinLink> JoinClubRoomAsync(int clubId, int activityId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="activityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetClubRoomPasswordAsync(int clubId, int activityId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account. If the authenticated account is already using the club's tag, calling this again removes it.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubTagInfo> SetClubTagAsync(int clubId, CancellationToken cancellationToken = default);

    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="creation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Club> CreateClubAsync(ClubCreation creation, CancellationToken cancellationToken = default);
    Task<Club> EditClubAsync(int clubId, ClubEdition edition, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes all of the club's content, which cannot be recovered afterwards. Only the club creator can delete a club.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteClubAsync(int clubId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Campaigns created using this endpoint are deactivated by default. Use <see cref="EditClubActivityAsync"/> to activate them.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="creation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubCampaign> CreateClubCampaignAsync(int clubId, ClubCampaignCreation creation, CancellationToken cancellationToken = default);
    Task<ClubRoom> CreateClubRoomAsync(int clubId, ClubRoomCreation creation, CancellationToken cancellationToken = default);
    Task<ClubRoom> EditClubRoomAsync(int clubId, int activityId, ClubRoomEdition edition, CancellationToken cancellationToken = default);
    Task<ClubNews> CreateClubNewsAsync(int clubId, ClubNewsCreation creation, CancellationToken cancellationToken = default);
    Task<ClubNews> EditClubNewsAsync(int clubId, int activityId, ClubNewsEdition edition, CancellationToken cancellationToken = default);
    /// <summary>
    /// The <see cref="ClubRankingCreation.UseCase"/> supports "ranking-official" (current quarterly campaign), "ranking-daily" (Daily Track / TOTD), and "ranking-club" (club campaign, requires <see cref="ClubRankingCreation.CampaignId"/> to be set to a campaign from the club).
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="creation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubRanking> CreateClubRankingAsync(int clubId, ClubRankingCreation creation, CancellationToken cancellationToken = default);
    Task<ClubMapReviewRoom> CreateClubMapReviewActivityAsync(int clubId, ClubMapReviewCreation creation, CancellationToken cancellationToken = default);
    Task<ClubMapReviewRoom> EditClubMapReviewActivityAsync(int clubId, int activityId, ClubMapReviewEdition edition, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request is only useful for clubs with their privacy level set to "private-open". For other privacy settings, this returns an empty list.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubMemberCollection> GetClubMemberRequestsAsync(int clubId, int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<ClubMember> SetClubMemberVipAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default);
    Task<ClubMember> UnsetClubMemberVipAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default);
    /// <summary>
    /// The role supports "Member", "Content Creator", and "Admin".
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="accountId"></param>
    /// <param name="edition"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClubMember> EditClubMemberAsync(int clubId, Guid accountId, ClubMemberEdition edition, CancellationToken cancellationToken = default);
    /// <summary>
    /// Can also be used for the currently authenticated account to leave a club they don't own.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteClubMemberAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default);
    Task<ClubBucket> CreateClubUploadActivityAsync(int clubId, ClubBucketCreation creation, CancellationToken cancellationToken = default);
    /// <summary>
    /// Assets used for this endpoint must be uploaded to Nadeo's servers beforehand. Depending on the upload activity type, <paramref name="itemIds"/> should contain `mapUid`s, `skinId`s, or `itemId`s.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="activityId"></param>
    /// <param name="itemIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddToClubUploadActivityAsync(int clubId, int activityId, IEnumerable<string> itemIds, CancellationToken cancellationToken = default);
    Task RemoveFromClubUploadActivityAsync(int clubId, int activityId, IEnumerable<string> itemIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="activityId"></param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="withFeedback">Whether to include detailed statistics for votes.</param>
    /// <param name="withMapInfo">Whether to include detailed info about each map.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SubmittedMapCollection> GetClubSubmittedMapReviewMapsAsync(int clubId, int activityId, int length, int offset = 0, bool? withFeedback = null, bool? withMapInfo = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="reviewType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MapReviewConnectInfo> GetMapReviewConnectionAsync(string reviewType, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request is only useful with tokens authenticated through a Ubisoft user account with enough permissions to manage map review activities in the club.
    /// </summary>
    /// <param name="clubId"></param>
    /// <param name="activityId"></param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SubmittedMapCollection> GetClubMapReviewSubmissionsAsync(int clubId, int activityId, int length, int offset = 0, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="reviewType"></param>
    /// <param name="length"></param>
    /// <param name="offset"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SubmittedMapCollection> GetSubmittedMapReviewMapsAsync(string reviewType, int length, int offset = 0, CancellationToken cancellationToken = default);
    Task<int> GetMapReviewWaitingTimeAsync(string reviewType, CancellationToken cancellationToken = default);
}

public class NadeoLiveServices : NadeoAPI, INadeoLiveServices
{
    public override string Audience => nameof(NadeoLiveServices);
    public override string BaseAddress => "https://live-services.trackmania.nadeo.live/api";

    public NadeoLiveServices(HttpClient client, NadeoAPIHandler? handler = null, bool automaticallyAuthorize = true)
        : base(client, handler ?? new NadeoAPIHandler(), automaticallyAuthorize)
    {
    }

    public NadeoLiveServices(bool automaticallyAuthorize = true) : this(new HttpClient(), new NadeoAPIHandler(), automaticallyAuthorize)
    {
    }

    public virtual async Task<MapInfoLive?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        try
        {
            return await GetJsonAsync($"token/map/{mapUid}", NadeoAPIJsonContext.Default.MapInfoLive, cancellationToken);
        }
        catch (NadeoAPIResponseException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public virtual async Task<ImmutableList<MapInfoLive>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"token/map/get-multiple?mapUidList={string.Join(',', mapUids)}",
            NadeoAPIJsonContext.Default.MapInfoLiveCollection, cancellationToken)).MapList;
    }

    public async Task<ImmutableList<MapInfoLive>> GetMapInfosAsync(params string[] mapUids)
    {
        return await GetMapInfosAsync(mapUids, cancellationToken: default);
    }

    public virtual async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"token/leaderboard/group/Personal_Best/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}",
            NadeoAPIJsonContext.Default.TopLeaderboardCollection, cancellationToken);
    }

    public virtual async Task<TopLeaderboardCollection> GetTopLeaderboardAsync(string mapUid, string groupUid, int length = 10, int offset = 0, bool onlyWorld = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/map/{mapUid}/top?length={length}&offset={offset}&onlyWorld={onlyWorld}",
            NadeoAPIJsonContext.Default.TopLeaderboardCollection, cancellationToken);
    }

    public async Task<ImmutableList<Position>> GetLeaderboardPositionByTimeAsync(string mapUid, string groupUid, int score, CancellationToken cancellationToken = default)
    {
        return await GetLeaderboardPositionsByTimeAsync([mapUid], [groupUid], [score], cancellationToken);
    }

    public async Task<ImmutableList<Position>> GetLeaderboardPositionByTimeAsync(string mapUid, int score, CancellationToken cancellationToken = default)
    {
        return await GetLeaderboardPositionByTimeAsync(mapUid, "Personal_Best", score, cancellationToken);
    }

    public async Task<ImmutableList<Position>> GetLeaderboardPositionsByTimeAsync(IEnumerable<string> mapUids, IEnumerable<int> scores, CancellationToken cancellationToken = default)
    {
        return await GetLeaderboardPositionsByTimeAsync(mapUids, ["Personal_Best"], scores, cancellationToken);
    }

    public virtual async Task<ImmutableList<Position>> GetLeaderboardPositionsByTimeAsync(IEnumerable<string> mapUids, IEnumerable<string> groupUids, IEnumerable<int> scores, CancellationToken cancellationToken = default)
    {
        var body = new MapGroupIdCollection(mapUids
            .Zip(groupUids, (mapUid, groupUid) =>
                new MapGroupId(mapUid, groupUid))
            .ToImmutableList());
        var jsonContent = JsonContent.Create(body, NadeoAPIJsonContext.Default.MapGroupIdCollection);
        
        var queryParams = mapUids
            .Zip(scores, (mapUid, score) =>
                $"scores[{mapUid}]={score}")
            .Aggregate((a, b) => $"{a}&{b}");
        
        try
        {
            return await PostJsonAsync($"token/leaderboard/group/map?{queryParams}", jsonContent, 
                NadeoAPIJsonContext.Default.ImmutableListPosition, cancellationToken);
        }
        catch (NadeoAPIResponseException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return [];
        }
    }

    public virtual async Task<ImmutableList<Maniapub>> GetActiveManiapubsAsync(CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"token/advertising/display/active", NadeoAPIJsonContext.Default.ManiapubCollection, cancellationToken)).DisplayList;
    }

    public virtual async Task<MedalRecordCollection> GetMapMedalRecordsAsync(string mapUid, string groupUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);
        ArgumentException.ThrowIfNullOrEmpty(groupUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/map/{mapUid}/medals",
            NadeoAPIJsonContext.Default.MedalRecordCollection, cancellationToken);
    }

    public virtual async Task<MedalRecordCollection> GetMapMedalRecordsAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

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
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"campaign/map/{mapUid}", NadeoAPIJsonContext.Default.TrackOfTheDayInfo, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetSeasonalCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/official?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<SeasonPlayerRankingCollection> GetPlayerSeasonRankingsAsync(Guid accountId, string groupUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}?accountId={accountId}",
            NadeoAPIJsonContext.Default.SeasonPlayerRankingCollection, cancellationToken);
    }

    [Obsolete("Use GetWeeklyShortCampaignsAsync instead.")]
    public virtual async Task<CampaignCollection> GetWeeklyCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetWeeklyShortCampaignsAsync(length, offset, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetWeeklyShortCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/weekly-shorts?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetWeeklyGrandCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/weekly-grands?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<ClubMember> GetClubMemberAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/member/{accountId}",
            NadeoAPIJsonContext.Default.ClubMember, cancellationToken);
    }

    public virtual async Task<ClubMember> GetClubMemberAsync(int clubId, string displayName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(displayName);

        return await GetJsonAsync($"token/club/{clubId}/member/{displayName}/from-login",
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
        ArgumentException.ThrowIfNullOrEmpty(folderName);

        var jsonContent = JsonContent.Create(new ClubFolder(folderName, FolderId: 0), NadeoAPIJsonContext.Default.ClubFolder);
        return await PostJsonAsync($"token/club/{clubId}/folder/create", jsonContent, NadeoAPIJsonContext.Default.ClubActivity, cancellationToken: cancellationToken);
    }

    public virtual async Task DeleteClubActivityAsync(int clubId, int activityId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/activity/{activityId}/delete", cancellationToken: cancellationToken);
        // Response: OK Activity deleted
    }

    public virtual async Task<string> JoinDailyChannelAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "token/channel/daily/join", cancellationToken: cancellationToken);
        return (await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.DailyChannelJoin, cancellationToken))?.JoinLink ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<PlayerServerAccountCollection> GetDedicatedServerAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("token/server/player-server/account", NadeoAPIJsonContext.Default.PlayerServerAccountCollection, cancellationToken);
    }

    public virtual async Task<CampaignCollection> GetCupOfTheWeekCampaignsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"campaign/cup-of-the-week?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.CampaignCollection, cancellationToken);
    }

    public virtual async Task<CampaignRankingCollection> GetCampaignLeaderboardAsync(string groupUid, int length = 5, int offset = 0, bool? onlyWorld = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/top?length={length}&offset={offset}{(onlyWorld is null ? "" : $"&onlyWorld={onlyWorld}")}",
            NadeoAPIJsonContext.Default.CampaignRankingCollection, cancellationToken);
    }

    public virtual async Task<ClubCampaignTopCollection> GetClubCampaignLeaderboardAsync(string groupUid, int clubId, int length = 5, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/club/{clubId}/top?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubCampaignTopCollection, cancellationToken);
    }

    public virtual async Task<ClubCampaignRanking> GetPlayerClubCampaignRankingAsync(string groupUid, int clubId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/club/{clubId}",
            NadeoAPIJsonContext.Default.ClubCampaignRanking, cancellationToken);
    }

    public virtual async Task<ClubMapRecord> GetPlayerClubMapRecordAsync(string groupUid, string mapUid, int clubId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/map/{mapUid}/club/{clubId}",
            NadeoAPIJsonContext.Default.ClubMapRecord, cancellationToken);
    }

    public async Task<ClubMapRecord> GetPlayerClubMapRecordAsync(string mapUid, int clubId, CancellationToken cancellationToken = default)
    {
        return await GetPlayerClubMapRecordAsync("Personal_Best", mapUid, clubId, cancellationToken);
    }

    public virtual async Task<ImmutableList<Position>> GetPlayerRecordsAsync(IEnumerable<string> mapUids, IEnumerable<string> groupUids, CancellationToken cancellationToken = default)
    {
        var body = new MapGroupIdCollection(mapUids
            .Zip(groupUids, (mapUid, groupUid) =>
                new MapGroupId(mapUid, groupUid))
            .ToImmutableList());
        var jsonContent = JsonContent.Create(body, NadeoAPIJsonContext.Default.MapGroupIdCollection);

        return await PostJsonAsync("token/leaderboard/group/map", jsonContent,
            NadeoAPIJsonContext.Default.ImmutableListPosition, cancellationToken);
    }

    public async Task<ImmutableList<Position>> GetPlayerRecordsAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        var mapUidList = mapUids.ToImmutableList();
        return await GetPlayerRecordsAsync(mapUidList, mapUidList.Select(_ => "Personal_Best"), cancellationToken);
    }

    public virtual async Task<TopLeaderboardCollection> GetSurroundingRecordsAsync(string groupUid, string mapUid, int lower, int upper, int? score = null, bool? onlyWorld = null, string? zoneId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/map/{mapUid}/surround/{lower}/{upper}?" +
            $"{(score is null ? "" : $"score={score}&")}" +
            $"{(onlyWorld is null ? "" : $"onlyWorld={onlyWorld}&")}" +
            $"{(zoneId is null ? "" : $"zoneId={zoneId}")}",
            NadeoAPIJsonContext.Default.TopLeaderboardCollection, cancellationToken);
    }

    public virtual async Task<ClubTopLeaderboard> GetClubTopLeaderboardAsync(string groupUid, string mapUid, int clubId, int length = 5, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupUid);
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"token/leaderboard/group/{groupUid}/map/{mapUid}/club/{clubId}/top?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubTopLeaderboard, cancellationToken);
    }

    public async Task<ClubTopLeaderboard> GetClubTopLeaderboardAsync(string mapUid, int clubId, int length = 5, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetClubTopLeaderboardAsync("Personal_Best", mapUid, clubId, length, offset, cancellationToken);
    }

    public virtual async Task<TrophyRankingCollection> GetPlayerTrophyRankingsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        var body = new PlayerListRequest(accountIds.Select(x => new PlayerIdRequest(x)).ToImmutableList());
        var jsonContent = JsonContent.Create(body, NadeoAPIJsonContext.Default.PlayerListRequest);

        return await PostJsonAsync("token/leaderboard/trophy/player", jsonContent,
            NadeoAPIJsonContext.Default.TrophyRankingCollection, cancellationToken);
    }

    public async Task<TrophyRankingCollection> GetPlayerTrophyRankingsAsync(params Guid[] accountIds)
    {
        return await GetPlayerTrophyRankingsAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<TrophyPlayerRanking> GetOwnTrophyRankingAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("token/leaderboard/trophy", NadeoAPIJsonContext.Default.TrophyPlayerRanking, cancellationToken);
    }

    public virtual async Task AddFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        using var response = await SendAsync(HttpMethod.Post, $"token/map/favorite/{mapUid}/add", cancellationToken: cancellationToken);
    }

    public virtual async Task RemoveFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        using var response = await SendAsync(HttpMethod.Post, $"token/map/favorite/{mapUid}/remove", cancellationToken: cancellationToken);
    }

    public virtual async Task<MapInfoLiveCollection> GetFavoriteMapsAsync(int length, int offset = 0, string sort = "date", string order = "desc", string? mapTypeList = null, bool? playable = null, bool? onlyMine = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/map/favorite?offset={offset}&length={length}&sort={sort}&order={order}" +
            $"{(mapTypeList is null ? "" : $"&mapTypeList={mapTypeList}")}" +
            $"{(playable is null ? "" : $"&playable={playable}")}" +
            $"{(onlyMine is null ? "" : $"&onlyMine={onlyMine}")}",
            NadeoAPIJsonContext.Default.MapInfoLiveCollection, cancellationToken);
    }

    public virtual async Task<MapInfoLiveCollection> GetUploadedMapsAsync(int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/map?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.MapInfoLiveCollection, cancellationToken);
    }

    public virtual async Task<ClubMapReviewRoom> GetClubMapReviewActivityAsync(int clubId, int activityId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/map-review/{activityId}",
            NadeoAPIJsonContext.Default.ClubMapReviewRoom, cancellationToken);
    }

    public virtual async Task<ClubMember> JoinClubAsync(int clubId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/member/create", cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.ClubMember, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<ClubNews> GetClubNewsAsync(int clubId, int activityId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/news/{activityId}", NadeoAPIJsonContext.Default.ClubNews, cancellationToken);
    }

    public virtual async Task<ClubTagInfo> PinClubAsync(int clubId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/pin", cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.ClubTagInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<ClubRoomJoinLink> JoinClubRoomAsync(int clubId, int activityId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/room/{activityId}/join", cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.ClubRoomJoinLink, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<string> GetClubRoomPasswordAsync(int clubId, int activityId, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"token/club/{clubId}/room/{activityId}/get-password",
            NadeoAPIJsonContext.Default.ClubRoomPassword, cancellationToken)).Password;
    }

    public virtual async Task<ClubTagInfo> SetClubTagAsync(int clubId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/tag", cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.ClubTagInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<Club> CreateClubAsync(ClubCreation creation, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(creation, NadeoAPIJsonContext.Default.ClubCreation);
        return await PostJsonAsync("token/club/create", jsonContent, NadeoAPIJsonContext.Default.Club, cancellationToken);
    }

    public virtual async Task<Club> EditClubAsync(int clubId, ClubEdition edition, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(edition, NadeoAPIJsonContext.Default.ClubEdition);
        return await PostJsonAsync($"token/club/{clubId}/edit", jsonContent, NadeoAPIJsonContext.Default.Club, cancellationToken);
    }

    public virtual async Task DeleteClubAsync(int clubId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/delete", cancellationToken: cancellationToken);
    }

    public virtual async Task<ClubCampaign> CreateClubCampaignAsync(int clubId, ClubCampaignCreation creation, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(creation, NadeoAPIJsonContext.Default.ClubCampaignCreation);
        return await PostJsonAsync($"token/club/{clubId}/campaign/create", jsonContent, NadeoAPIJsonContext.Default.ClubCampaign, cancellationToken);
    }

    public virtual async Task<ClubRoom> CreateClubRoomAsync(int clubId, ClubRoomCreation creation, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(creation, NadeoAPIJsonContext.Default.ClubRoomCreation);
        return await PostJsonAsync($"token/club/{clubId}/room/create", jsonContent, NadeoAPIJsonContext.Default.ClubRoom, cancellationToken);
    }

    public virtual async Task<ClubRoom> EditClubRoomAsync(int clubId, int activityId, ClubRoomEdition edition, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(edition, NadeoAPIJsonContext.Default.ClubRoomEdition);
        return await PostJsonAsync($"token/club/{clubId}/room/{activityId}/edit", jsonContent, NadeoAPIJsonContext.Default.ClubRoom, cancellationToken);
    }

    public virtual async Task<ClubNews> CreateClubNewsAsync(int clubId, ClubNewsCreation creation, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(creation, NadeoAPIJsonContext.Default.ClubNewsCreation);
        return await PostJsonAsync($"token/club/{clubId}/news/create", jsonContent, NadeoAPIJsonContext.Default.ClubNews, cancellationToken);
    }

    public virtual async Task<ClubNews> EditClubNewsAsync(int clubId, int activityId, ClubNewsEdition edition, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(edition, NadeoAPIJsonContext.Default.ClubNewsEdition);
        return await PostJsonAsync($"token/club/{clubId}/news/{activityId}/edit", jsonContent, NadeoAPIJsonContext.Default.ClubNews, cancellationToken);
    }

    public virtual async Task<ClubRanking> CreateClubRankingAsync(int clubId, ClubRankingCreation creation, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(creation, NadeoAPIJsonContext.Default.ClubRankingCreation);
        return await PostJsonAsync($"token/club/{clubId}/ranking/create", jsonContent, NadeoAPIJsonContext.Default.ClubRanking, cancellationToken);
    }

    public virtual async Task<ClubMapReviewRoom> CreateClubMapReviewActivityAsync(int clubId, ClubMapReviewCreation creation, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(creation, NadeoAPIJsonContext.Default.ClubMapReviewCreation);
        return await PostJsonAsync($"token/club/{clubId}/map-review/create", jsonContent, NadeoAPIJsonContext.Default.ClubMapReviewRoom, cancellationToken);
    }

    public virtual async Task<ClubMapReviewRoom> EditClubMapReviewActivityAsync(int clubId, int activityId, ClubMapReviewEdition edition, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(edition, NadeoAPIJsonContext.Default.ClubMapReviewEdition);
        return await PostJsonAsync($"token/club/{clubId}/map-review/{activityId}/edit", jsonContent, NadeoAPIJsonContext.Default.ClubMapReviewRoom, cancellationToken);
    }

    public virtual async Task<ClubMemberCollection> GetClubMemberRequestsAsync(int clubId, int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/member/request?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.ClubMemberCollection, cancellationToken);
    }

    public virtual async Task<ClubMember> SetClubMemberVipAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/vip/{accountId}/set", cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.ClubMember, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<ClubMember> UnsetClubMemberVipAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/vip/{accountId}/unset", cancellationToken: cancellationToken);
        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.ClubMember, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    public virtual async Task<ClubMember> EditClubMemberAsync(int clubId, Guid accountId, ClubMemberEdition edition, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(edition, NadeoAPIJsonContext.Default.ClubMemberEdition);
        return await PostJsonAsync($"token/club/{clubId}/member/{accountId}/edit", jsonContent, NadeoAPIJsonContext.Default.ClubMember, cancellationToken);
    }

    public virtual async Task DeleteClubMemberAsync(int clubId, Guid accountId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/member/{accountId}/delete", cancellationToken: cancellationToken);
    }

    public virtual async Task<ClubBucket> CreateClubUploadActivityAsync(int clubId, ClubBucketCreation creation, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(creation, NadeoAPIJsonContext.Default.ClubBucketCreation);
        return await PostJsonAsync($"token/club/{clubId}/bucket/create", jsonContent, NadeoAPIJsonContext.Default.ClubBucket, cancellationToken);
    }

    public virtual async Task AddToClubUploadActivityAsync(int clubId, int activityId, IEnumerable<string> itemIds, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(new ClubBucketItemIdList(itemIds.ToImmutableList()), NadeoAPIJsonContext.Default.ClubBucketItemIdList);
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/bucket/{activityId}/add", jsonContent, cancellationToken);
    }

    public virtual async Task RemoveFromClubUploadActivityAsync(int clubId, int activityId, IEnumerable<string> itemIds, CancellationToken cancellationToken = default)
    {
        var jsonContent = JsonContent.Create(new ClubBucketItemIdList(itemIds.ToImmutableList()), NadeoAPIJsonContext.Default.ClubBucketItemIdList);
        using var response = await SendAsync(HttpMethod.Post, $"token/club/{clubId}/bucket/{activityId}/remove", jsonContent, cancellationToken);
    }

    public virtual async Task<SubmittedMapCollection> GetClubSubmittedMapReviewMapsAsync(int clubId, int activityId, int length, int offset = 0, bool? withFeedback = null, bool? withMapInfo = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/map-review/{activityId}/map/mine?length={length}&offset={offset}" +
            $"{(withFeedback is null ? "" : $"&withFeedback={withFeedback}")}" +
            $"{(withMapInfo is null ? "" : $"&withMapInfo={withMapInfo}")}",
            NadeoAPIJsonContext.Default.SubmittedMapCollection, cancellationToken);
    }

    public virtual async Task<MapReviewConnectInfo> GetMapReviewConnectionAsync(string reviewType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(reviewType);

        return await GetJsonAsync($"token/map-review/{reviewType}/connect", NadeoAPIJsonContext.Default.MapReviewConnectInfo, cancellationToken);
    }

    public virtual async Task<SubmittedMapCollection> GetClubMapReviewSubmissionsAsync(int clubId, int activityId, int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"token/club/{clubId}/map-review/{activityId}/map?offset={offset}&length={length}",
            NadeoAPIJsonContext.Default.SubmittedMapCollection, cancellationToken);
    }

    public virtual async Task<SubmittedMapCollection> GetSubmittedMapReviewMapsAsync(string reviewType, int length, int offset = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(reviewType);

        return await GetJsonAsync($"token/map/map-review/{reviewType}/submitted-map?length={length}&offset={offset}",
            NadeoAPIJsonContext.Default.SubmittedMapCollection, cancellationToken);
    }

    public virtual async Task<int> GetMapReviewWaitingTimeAsync(string reviewType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(reviewType);

        return (await GetJsonAsync($"token/map-review/{reviewType}/waiting-time",
            NadeoAPIJsonContext.Default.MapReviewWaitingTime, cancellationToken)).Seconds;
    }
}
