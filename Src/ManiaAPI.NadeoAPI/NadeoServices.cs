using System.Collections.Immutable;
using System.Net.Http.Json;

namespace ManiaAPI.NadeoAPI;

public interface INadeoServices : INadeoAPI
{
    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    Task<ImmutableList<Account>> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    Task<ImmutableList<Account>> GetAccountDisplayNamesAsync(params Guid[] accountIds);
    Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, string? seasonId = null, string? gamemode = null, CancellationToken cancellationToken = default);
	Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, Guid mapId, string? seasonId = null, string? gamemode = null, CancellationToken cancellationToken = default);
	Task<ImmutableList<MapRecord>> GetMapRecordsByIdsAsync(IEnumerable<Guid> mapRecordIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapRecord>> GetMapRecordsByIdsAsync(params Guid[] mapRecordIds);
    [Obsolete("Use GetAccountRecordsByMapIdsAsync or GetAccountRecordsBySeasonIdsAsync instead.")]
    Task<ImmutableList<MapRecord>> GetAccountRecordsAsync(Guid accountId, string? gamemode = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<ImmutableList<MapRecord>> GetAccountRecordsByMapIdsAsync(Guid accountId, IEnumerable<Guid> mapIds, string? gamemode = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="accountId">The ID of the account.</param>
    /// <param name="mapIds">The IDs of the maps.</param>
    /// <param name="gamemode">Sent as the <c>gameMode</c> query parameter. Left out of the request when null.</param>
    /// <param name="offset">The number of records to skip.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImmutableList<MapRecord>> GetAccountRecordsByMapIdsAsync(Guid accountId, IEnumerable<Guid> mapIds, string? gamemode, int offset, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<ImmutableList<MapRecord>> GetAccountRecordsBySeasonIdsAsync(Guid accountId, IEnumerable<Guid> seasonIds, string? gamemode = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="accountId">The ID of the account.</param>
    /// <param name="seasonIds">The IDs of the seasons.</param>
    /// <param name="gamemode">Sent as the <c>gameMode</c> query parameter. Left out of the request when null.</param>
    /// <param name="offset">The number of records to skip.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ImmutableList<MapRecord>> GetAccountRecordsBySeasonIdsAsync(Guid accountId, IEnumerable<Guid> seasonIds, string? gamemode, int offset, CancellationToken cancellationToken = default);
    Task<MapRecord> GetMapRecordByIdAsync(Guid mapRecordId, CancellationToken cancellationToken = default);
    Task<ImmutableList<PlayerZone>> GetPlayerZonesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<PlayerZone>> GetPlayerZonesAsync(params Guid[] accountIds);
    Task<PlayerZone?> GetPlayerZoneAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default);
    Task<ImmutableList<Zone>> GetZonesAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(params Guid[] accountIds);
    Task<MapInfo?> GetMapInfoAsync(Guid mapId, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
    Task<MapInfo?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(params Guid[] accountIds);
    Task<WebIdentity?> GetPlayerWebIdentityAsync(Guid accountId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<MapInfoCollection> GetMapsByAuthorAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<SkinInfo?> GetSkinInfoAsync(Guid skinId, CancellationToken cancellationToken = default);
    Task<ImmutableList<SkinIdentifier>> GetSkinsByAccountIdsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<SkinIdentifier>> GetSkinsByAccountIdsAsync(params Guid[] accountIds);
    Task<SkinIdentifier?> GetSkinByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="accountId">The ID of the account.</param>
    /// <param name="count">The number of entries to retrieve. Max allowed is 1000.</param>
    /// <param name="offset">The number of entries to skip (looking back from the most recent).</param>
    /// <param name="trophyType">The level of trophy to filter for (between 1 and 9).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<TrophyHistoryCollection> GetPlayerTrophyHistoryAsync(Guid accountId, int count = 100, int offset = 0, int? trophyType = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<TrophySummary> GetPlayerTrophySummaryAsync(Guid accountId, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task AddFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task RemoveFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="length">Number of maps to retrieve.</param>
    /// <param name="offset">Number of maps to skip.</param>
    /// <param name="sort">Either "date" or "name".</param>
    /// <param name="order">Either "asc" or "desc".</param>
    /// <param name="mapTypeList">Sent as the <c>mapTypeList</c> filter. Left out of the request when null.</param>
    /// <param name="playable">Sent as the <c>playable</c> filter. Left out of the request when null.</param>
    /// <param name="onlyMine">Sent as the <c>onlyMine</c> filter. Left out of the request when null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<MapFavoriteCollection> GetFavoriteMapsAsync(int length, int offset = 0, string sort = "date", string order = "desc", string? mapTypeList = null, bool? playable = null, bool? onlyMine = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<MapFavoriteCollection> GetFavoriteMapsByUidsAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    Task<MapFavoriteCollection> GetFavoriteMapsByUidsAsync(params string[] mapUids);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<MapVote> GetMapVoteAsync(string mapUid, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    /// <param name="mapUid">The UID of the map.</param>
    /// <param name="vote">-1 for a dislike, 1 for a like, 0 to unset the vote.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetMapVoteAsync(string mapUid, int vote, CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<MapInfoCollection> GetSubmittedMapsAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// This request requires authentication through service account.
    /// </summary>
    Task<ImmutableList<SkinFavorite>> GetFavoriteSkinsAsync(Guid accountId, CancellationToken cancellationToken = default);
}

public class NadeoServices : NadeoAPI, INadeoServices
{
    public override string Audience => nameof(NadeoServices);
    public override string BaseAddress => "https://prod.trackmania.core.nadeo.online";

    public NadeoServices(HttpClient client, NadeoAPIHandler? handler = null, bool automaticallyAuthorize = true)
        : base(client, handler ?? new NadeoAPIHandler(), automaticallyAuthorize)
    {
    }

    public NadeoServices(bool automaticallyAuthorize = true) : this(new HttpClient(), new NadeoAPIHandler(), automaticallyAuthorize)
    {
	}

	public virtual async Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, Guid mapId, string? seasonId = null, string? gamemode = null, CancellationToken cancellationToken = default)
	{
		return await GetJsonAsync($"v2/mapRecords/by-account/?accountIdList={string.Join(',', accountIds)}&mapId={mapId}{(seasonId is null ? null : $"&seasonId={Uri.EscapeDataString(seasonId)}")}{(gamemode is null ? null : $"&gameMode={Uri.EscapeDataString(gamemode)}")}",
			NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
	}

	public virtual async Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, string? seasonId = null, string? gamemode = null, CancellationToken cancellationToken = default)
    {
        var records = ImmutableList.CreateBuilder<MapRecord>();

        foreach (var mapId in mapIds)
		{
			records.AddRange(await GetMapRecordsAsync(accountIds, mapId, seasonId, gamemode, cancellationToken));
		}

        return records.ToImmutable();
    }

    public virtual async Task<ImmutableList<MapRecord>> GetMapRecordsByIdsAsync(IEnumerable<Guid> mapRecordIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/mapRecords/by-id/?mapRecordIdList={string.Join(',', mapRecordIds)}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public async Task<ImmutableList<MapRecord>> GetMapRecordsByIdsAsync(params Guid[] mapRecordIds)
    {
        return await GetMapRecordsByIdsAsync(mapRecordIds, CancellationToken.None);
    }

    [Obsolete("Use GetAccountRecordsByMapIdsAsync or GetAccountRecordsBySeasonIdsAsync instead.")]
    public virtual async Task<ImmutableList<MapRecord>> GetAccountRecordsAsync(Guid accountId, string? gamemode = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords/{(gamemode is null ? null : $"?gameMode={Uri.EscapeDataString(gamemode)}")}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<ImmutableList<MapRecord>> GetAccountRecordsByMapIdsAsync(Guid accountId, IEnumerable<Guid> mapIds, string? gamemode = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords/?mapIdList={string.Join(',', mapIds)}{(gamemode is null ? null : $"&gameMode={Uri.EscapeDataString(gamemode)}")}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<ImmutableList<MapRecord>> GetAccountRecordsBySeasonIdsAsync(Guid accountId, IEnumerable<Guid> seasonIds, string? gamemode = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords/?seasonIdList={string.Join(',', seasonIds)}{(gamemode is null ? null : $"&gameMode={Uri.EscapeDataString(gamemode)}")}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<ImmutableList<MapRecord>> GetAccountRecordsByMapIdsAsync(Guid accountId, IEnumerable<Guid> mapIds, string? gamemode, int offset, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords/?mapIdList={string.Join(',', mapIds)}{(gamemode is null ? null : $"&gameMode={Uri.EscapeDataString(gamemode)}")}&offset={offset}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<ImmutableList<MapRecord>> GetAccountRecordsBySeasonIdsAsync(Guid accountId, IEnumerable<Guid> seasonIds, string? gamemode, int offset, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords/?seasonIdList={string.Join(',', seasonIds)}{(gamemode is null ? null : $"&gameMode={Uri.EscapeDataString(gamemode)}")}&offset={offset}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<MapRecord> GetMapRecordByIdAsync(Guid mapRecordId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"mapRecords/{mapRecordId}", NadeoAPIJsonContext.Default.MapRecord, cancellationToken);
    }

    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    public virtual async Task<ImmutableList<Account>> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/displayNames/?accountIdList={string.Join(',', accountIds)}",
            NadeoAPIJsonContext.Default.ImmutableListAccount, cancellationToken);
    }

    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    public async Task<ImmutableList<Account>> GetAccountDisplayNamesAsync(params Guid[] accountIds)
    {
        return await GetAccountDisplayNamesAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<ImmutableList<PlayerZone>> GetPlayerZonesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/zones/?accountIdList={string.Join(',', accountIds)}",
            NadeoAPIJsonContext.Default.ImmutableListPlayerZone, cancellationToken);
    }

    public async Task<ImmutableList<PlayerZone>> GetPlayerZonesAsync(params Guid[] accountIds)
    {
        return await GetPlayerZonesAsync(accountIds, CancellationToken.None);
    }

    public async Task<PlayerZone?> GetPlayerZoneAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return (await GetPlayerZonesAsync([accountId], cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"api/routes?usage={usage}", NadeoAPIJsonContext.Default.DictionaryStringApiRoute, cancellationToken);
    }

    public virtual async Task<ImmutableList<Zone>> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("zones/", NadeoAPIJsonContext.Default.ImmutableListZone, cancellationToken);
    }

    public async Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/clubTags/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.ImmutableListPlayerClubTag, cancellationToken);
    }

    public async Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(params Guid[] accountIds)
    {
        return await GetPlayerClubTagsAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/by-id/?mapIdList={string.Join(',', mapIds)}",
            NadeoAPIJsonContext.Default.ImmutableListMapInfo, cancellationToken);
    }

    public async Task<MapInfo?> GetMapInfoAsync(Guid mapId, CancellationToken cancellationToken = default)
    {
        return (await GetMapInfosAsync([mapId], cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/by-uid/?mapUidList={string.Join(',', mapUids)}",
            NadeoAPIJsonContext.Default.ImmutableListMapInfo, cancellationToken);
    }

    public virtual async Task<MapInfo?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        return (await GetMapInfosAsync([mapUid], cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"webidentities/by-account/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.ImmutableListWebIdentity, cancellationToken);
    }

    public async Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(params Guid[] accountIds)
    {
        return await GetPlayerWebIdentitiesAsync(accountIds, CancellationToken.None);
    }

    public async Task<WebIdentity?> GetPlayerWebIdentityAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return (await GetPlayerWebIdentitiesAsync([accountId], cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<MapInfoCollection> GetMapsByAuthorAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("maps/by-author/", NadeoAPIJsonContext.Default.MapInfoCollection, cancellationToken);
    }

    public virtual async Task<SkinInfo?> GetSkinInfoAsync(Guid skinId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetJsonAsync($"skins/{skinId}", NadeoAPIJsonContext.Default.SkinInfo, cancellationToken);
        }
        catch (NadeoAPIResponseException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public virtual async Task<ImmutableList<SkinIdentifier>> GetSkinsByAccountIdsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/skins/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.ImmutableListSkinIdentifier, cancellationToken);
    }

    public async Task<ImmutableList<SkinIdentifier>> GetSkinsByAccountIdsAsync(params Guid[] accountIds)
    {
        return await GetSkinsByAccountIdsAsync(accountIds, CancellationToken.None);
    }

    public async Task<SkinIdentifier?> GetSkinByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return (await GetSkinsByAccountIdsAsync([accountId], cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<TrophyHistoryCollection> GetPlayerTrophyHistoryAsync(Guid accountId, int count = 100, int offset = 0, int? trophyType = null, CancellationToken cancellationToken = default)
    {
        // Without the slash before the query, the API answers with a redirect to this route
        return await GetJsonAsync($"accounts/{accountId}/trophies/?count={count}&offset={offset}{(trophyType is null ? "" : $"&trophyType={trophyType}")}",
            NadeoAPIJsonContext.Default.TrophyHistoryCollection, cancellationToken);
    }

    public virtual async Task<TrophySummary> GetPlayerTrophySummaryAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/{accountId}/trophies/lastYearSummary", NadeoAPIJsonContext.Default.TrophySummary, cancellationToken);
    }

    public virtual async Task AddFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        var jsonContent = JsonContent.Create(new MapUidRequest(mapUid), NadeoAPIJsonContext.Default.MapUidRequest);
        using var response = await SendAsync(HttpMethod.Post, "maps/favorites", jsonContent, cancellationToken);
    }

    public virtual async Task RemoveFavoriteMapAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        var jsonContent = JsonContent.Create(new MapUidRequest(mapUid), NadeoAPIJsonContext.Default.MapUidRequest);
        using var response = await SendAsync(HttpMethod.Delete, "maps/favorites", jsonContent, cancellationToken);
    }

    public virtual async Task<MapFavoriteCollection> GetFavoriteMapsAsync(int length, int offset = 0, string sort = "date", string order = "desc", string? mapTypeList = null, bool? playable = null, bool? onlyMine = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/favorites?offset={offset}&length={length}&sort={Uri.EscapeDataString(sort)}&order={Uri.EscapeDataString(order)}" +
            $"{(mapTypeList is null ? "" : $"&mapTypeList={Uri.EscapeDataString(mapTypeList)}")}" +
            $"{(playable is null ? "" : $"&playable={playable}")}" +
            $"{(onlyMine is null ? "" : $"&onlyMine={onlyMine}")}",
            NadeoAPIJsonContext.Default.MapFavoriteCollection, cancellationToken);
    }

    public virtual async Task<MapFavoriteCollection> GetFavoriteMapsByUidsAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/favorites/by-map-uids?mapUidList={string.Join(',', mapUids)}",
            NadeoAPIJsonContext.Default.MapFavoriteCollection, cancellationToken);
    }

    public async Task<MapFavoriteCollection> GetFavoriteMapsByUidsAsync(params string[] mapUids)
    {
        return await GetFavoriteMapsByUidsAsync(mapUids, CancellationToken.None);
    }

    public virtual async Task<MapVote> GetMapVoteAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        return await GetJsonAsync($"maps/{UrlHelper.EscapePathSegment(mapUid)}/votes", NadeoAPIJsonContext.Default.MapVote, cancellationToken);
    }

    public virtual async Task SetMapVoteAsync(string mapUid, int vote, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapUid);

        var jsonContent = JsonContent.Create(new MapVoteRequest(vote), NadeoAPIJsonContext.Default.MapVoteRequest);
        using var response = await SendAsync(HttpMethod.Post, $"maps/{UrlHelper.EscapePathSegment(mapUid)}/votes", jsonContent, cancellationToken);
    }

    public virtual async Task<MapInfoCollection> GetSubmittedMapsAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("maps/by-submitter/", NadeoAPIJsonContext.Default.MapInfoCollection, cancellationToken);
    }

    public virtual async Task<ImmutableList<SkinFavorite>> GetFavoriteSkinsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/{accountId}/skins/favorites/", NadeoAPIJsonContext.Default.ImmutableListSkinFavorite, cancellationToken);
    }
}
