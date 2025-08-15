using ManiaAPI.NadeoAPI.JsonContexts;
using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public interface INadeoServices : INadeoAPI
{
    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    Task<ImmutableArray<Account>> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    Task<ImmutableArray<Account>> GetAccountDisplayNamesAsync(params Guid[] accountIds);
    Task<ImmutableArray<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
	Task<ImmutableArray<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, Guid mapId, CancellationToken cancellationToken = default);
    Task<ImmutableArray<MapRecord>> GetAccountRecordsAsync(Guid accountId, string? gamemode = null, CancellationToken cancellationToken = default);
    Task<MapRecord> GetMapRecordByIdAsync(Guid mapRecordId, CancellationToken cancellationToken = default);
    Task<ImmutableArray<PlayerZone>> GetPlayerZonesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableArray<PlayerZone>> GetPlayerZonesAsync(params Guid[] accountIds);
    Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default);
    Task<ImmutableArray<Zone>> GetZonesAsync(CancellationToken cancellationToken = default);
    Task<ImmutableArray<PlayerClubTag>> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableArray<PlayerClubTag>> GetPlayerClubTagsAsync(params Guid[] accountIds);
    Task<MapInfo?> GetMapInfoAsync(Guid mapId, CancellationToken cancellationToken = default);
    Task<ImmutableArray<MapInfo>> GetMapInfosAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
    Task<MapInfo?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default);
    Task<ImmutableArray<MapInfo>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default);
    Task<ImmutableArray<WebIdentity>> GetPlayerWebIdentitiesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<ImmutableArray<WebIdentity>> GetPlayerWebIdentitiesAsync(params Guid[] accountIds);
    Task<MapInfoCollection> GetMapsByAuthor(CancellationToken cancellationToken = default);
    Task<SkinInfo> GetSkinInfoAsync(Guid skinId, CancellationToken cancellationToken = default);
}

public class NadeoServices : NadeoAPI, INadeoServices
{
    public override string Audience => nameof(NadeoServices);
    public override string BaseAddress => "https://prod.trackmania.core.nadeo.online";

    public NadeoServices(HttpClient client, NadeoAPIHandler handler, bool automaticallyAuthorize = true) : base(client, handler, automaticallyAuthorize)
    {
    }

    public NadeoServices(bool automaticallyAuthorize = true) : this(new HttpClient(), new NadeoAPIHandler(), automaticallyAuthorize)
    {
	}

	public virtual async Task<ImmutableArray<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, Guid mapId, CancellationToken cancellationToken = default)
	{
		return await GetJsonAsync($"v2/mapRecords/?accountIdList={string.Join(',', accountIds)}&mapId={mapId}",
			NadeoAPIJsonContext.Default.ImmutableArrayMapRecord, cancellationToken);
	}

	public virtual async Task<ImmutableArray<MapRecord>> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        var records = ImmutableArray.CreateBuilder<MapRecord>();

        foreach (var mapId in mapIds)
		{
			records.AddRange(await GetMapRecordsAsync(accountIds, mapId, cancellationToken));
		}

        return records.ToImmutable();
    }

    public virtual async Task<ImmutableArray<MapRecord>> GetAccountRecordsAsync(Guid accountId, string? gamemode = null, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"v2/accounts/{accountId}/mapRecords{(gamemode is null ? null : $"?gameMode={gamemode}")}",
            NadeoAPIJsonContext.Default.ImmutableArrayMapRecord, cancellationToken);
    }

    public virtual async Task<MapRecord> GetMapRecordByIdAsync(Guid mapRecordId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"mapRecords/{mapRecordId}", NadeoAPIJsonContext.Default.MapRecord, cancellationToken);
    }

    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    public virtual async Task<ImmutableArray<Account>> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/displayNames/?accountIdList={string.Join(',', accountIds)}",
            NadeoAPIJsonContext.Default.ImmutableArrayAccount, cancellationToken);
    }

    [Obsolete("Use ManiaAPI.TrackmaniaAPI to get the display names instead.")]
    public async Task<ImmutableArray<Account>> GetAccountDisplayNamesAsync(params Guid[] accountIds)
    {
        return await GetAccountDisplayNamesAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<ImmutableArray<PlayerZone>> GetPlayerZonesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/zones/?accountIdList={string.Join(',', accountIds)}",
            NadeoAPIJsonContext.Default.ImmutableArrayPlayerZone, cancellationToken);
    }

    public async Task<ImmutableArray<PlayerZone>> GetPlayerZonesAsync(params Guid[] accountIds)
    {
        return await GetPlayerZonesAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"api/routes?usage={usage}", NadeoAPIJsonContext.Default.DictionaryStringApiRoute, cancellationToken);
    }

    public virtual async Task<ImmutableArray<Zone>> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("zones", NadeoAPIJsonContext.Default.ImmutableArrayZone, cancellationToken);
    }

    public async Task<ImmutableArray<PlayerClubTag>> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/clubTags/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.ImmutableArrayPlayerClubTag, cancellationToken);
    }

    public async Task<ImmutableArray<PlayerClubTag>> GetPlayerClubTagsAsync(params Guid[] accountIds)
    {
        return await GetPlayerClubTagsAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<MapInfo?> GetMapInfoAsync(Guid mapId, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"maps/?mapIdList={mapId}", NadeoAPIJsonContext.Default.ImmutableArrayMapInfo, cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<ImmutableArray<MapInfo>> GetMapInfosAsync(IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/?mapIdList={string.Join(',', mapIds)}",
            NadeoAPIJsonContext.Default.ImmutableArrayMapInfo, cancellationToken);
    }

    public virtual async Task<MapInfo?> GetMapInfoAsync(string mapUid, CancellationToken cancellationToken = default)
    {
        return (await GetJsonAsync($"maps/?mapUidList={mapUid}", NadeoAPIJsonContext.Default.ImmutableArrayMapInfo, cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<ImmutableArray<MapInfo>> GetMapInfosAsync(IEnumerable<string> mapUids, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"maps/?mapUidList={string.Join(',', mapUids)}",
            NadeoAPIJsonContext.Default.ImmutableArrayMapInfo, cancellationToken);
    }

    public virtual async Task<ImmutableArray<WebIdentity>> GetPlayerWebIdentitiesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"webidentities/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.ImmutableArrayWebIdentity, cancellationToken);
    }

    public async Task<ImmutableArray<WebIdentity>> GetPlayerWebIdentitiesAsync(params Guid[] accountIds)
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
