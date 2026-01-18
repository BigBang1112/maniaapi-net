using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public interface INadeoServices : INadeoAPI
{
    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    Task<ImmutableList<Account>> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    Task<ImmutableList<Account>> GetAccountDisplayNamesAsync(params Guid[] accountIds);
    Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
	Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, Guid mapId, CancellationToken cancellationToken = default);
	Task<ImmutableList<MapRecord>> GetMapRecordsByIdsAsync(IEnumerable<Guid> mapRecordIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapRecord>> GetMapRecordsByIdsAsync(params Guid[] mapRecordIds);
    [Obsolete("Use GetAccountRecordsByMapIdsAsync or GetAccountRecordsBySeasonIdsAsync instead.")]
    Task<ImmutableList<MapRecord>> GetAccountRecordsAsync(Guid accountId, string? gamemode = null, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapRecord>> GetAccountRecordsByMapIdsAsync(Guid accountId, IEnumerable<Guid> mapIds, string? gamemode = null, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapRecord>> GetAccountRecordsBySeasonIdsAsync(Guid accountId, IEnumerable<Guid> seasonIds, string? gamemode = null, CancellationToken cancellationToken = default);
    Task<MapRecord> GetMapRecordByIdAsync(Guid mapRecordId, CancellationToken cancellationToken = default);
    Task<ImmutableList<PlayerZone>> GetPlayerZonesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<PlayerZone>> GetPlayerZonesAsync(params Guid[] accountIds);
    Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default);
    Task<ImmutableList<Zone>> GetZonesAsync(CancellationToken cancellationToken = default);
    Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(params Guid[] accountIds);
    Task<MapInfo?> GetMapInfoAsync(Guid mapId, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
    Task<MapInfo?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(params Guid[] accountIds);
    Task<MapInfoCollection> GetMapsByAuthor(CancellationToken cancellationToken = default);
    Task<SkinInfo> GetSkinInfoAsync(Guid skinId, CancellationToken cancellationToken = default);
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

	public virtual async Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, Guid mapId, CancellationToken cancellationToken = default)
	{
		return await GetJsonAsync($"v2/mapRecords/by-account/?accountIdList={string.Join(',', accountIds)}&mapId={mapId}",
			NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
	}

	public virtual async Task<ImmutableList<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        var records = ImmutableList.CreateBuilder<MapRecord>();

        foreach (var mapId in mapIds)
		{
			records.AddRange(await GetMapRecordsAsync(accountIds, mapId, cancellationToken));
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
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords{(gamemode is null ? null : $"?gameMode={gamemode}")}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<ImmutableList<MapRecord>> GetAccountRecordsByMapIdsAsync(Guid accountId, IEnumerable<Guid> mapIds, string? gamemode = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords?mapIdList={string.Join(',', mapIds)}{(gamemode is null ? null : $"&gameMode={gamemode}")}",
            NadeoAPIJsonContext.Default.ImmutableListMapRecord, cancellationToken);
    }

    public virtual async Task<ImmutableList<MapRecord>> GetAccountRecordsBySeasonIdsAsync(Guid accountId, IEnumerable<Guid> seasonIds, string? gamemode = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords?seasonIdList={string.Join(',', seasonIds)}{(gamemode is null ? null : $"&gameMode={gamemode}")}",
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

    public virtual async Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"api/routes?usage={usage}", NadeoAPIJsonContext.Default.DictionaryStringApiRoute, cancellationToken);
    }

    public virtual async Task<ImmutableList<Zone>> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("zones", NadeoAPIJsonContext.Default.ImmutableListZone, cancellationToken);
    }

    public async Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/clubTags/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.ImmutableListPlayerClubTag, cancellationToken);
    }

    public async Task<ImmutableList<PlayerClubTag>> GetPlayerClubTagsAsync(params Guid[] accountIds)
    {
        return await GetPlayerClubTagsAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<MapInfo?> GetMapInfoAsync(Guid mapId, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"maps/by-id/?mapIdList={mapId}", NadeoAPIJsonContext.Default.ImmutableListMapInfo, cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/by-id/?mapIdList={string.Join(',', mapIds)}",
            NadeoAPIJsonContext.Default.ImmutableListMapInfo, cancellationToken);
    }

    public virtual async Task<MapInfo?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"maps/by-uid/?mapUidList={mapUid}", NadeoAPIJsonContext.Default.ImmutableListMapInfo, cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<ImmutableList<MapInfo>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/by-uid/?mapUidList={string.Join(',', mapUids)}",
            NadeoAPIJsonContext.Default.ImmutableListMapInfo, cancellationToken);
    }

    public virtual async Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"webidentities/by-account/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.ImmutableListWebIdentity, cancellationToken);
    }

    public async Task<ImmutableList<WebIdentity>> GetPlayerWebIdentitiesAsync(params Guid[] accountIds)
    {
        return await GetPlayerWebIdentitiesAsync(accountIds, CancellationToken.None);
    }

    public async Task<MapInfoCollection> GetMapsByAuthor(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("maps/by-author", NadeoAPIJsonContext.Default.MapInfoCollection, cancellationToken);
    }

    public async Task<SkinInfo> GetSkinInfoAsync(Guid skinId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"skins/{skinId}", NadeoAPIJsonContext.Default.SkinInfo, cancellationToken);
    }
}
