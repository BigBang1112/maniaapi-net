using ManiaAPI.NadeoAPI.JsonContexts;

namespace ManiaAPI.NadeoAPI;

public interface INadeoServices : INadeoAPI
{
    Task<Account[]> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<Account[]> GetAccountDisplayNamesAsync(params Guid[] accountIds);
    Task<MapRecord[]> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
    Task<MapRecord> GetMapRecordByIdAsync(Guid mapRecordId, CancellationToken cancellationToken = default);
    Task<PlayerZone[]> GetPlayerZonesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<PlayerZone[]> GetPlayerZonesAsync(params Guid[] accountIds);
    Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default);
    Task<Zone[]> GetZonesAsync(CancellationToken cancellationToken = default);
    Task<PlayerClubTag[]> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<PlayerClubTag[]> GetPlayerClubTagsAsync(params Guid[] accountIds);
}

public class NadeoServices : NadeoAPI, INadeoServices
{
    public override string Audience => nameof(NadeoServices);
    public override string BaseAddress => "https://prod.trackmania.core.nadeo.online/";

    public NadeoServices(HttpClient client, bool automaticallyAuthorize = true) : base(client, automaticallyAuthorize)
    {
    }

    public NadeoServices(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize)
    {
    }

    public virtual async Task<MapRecord[]> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"mapRecords/?accountIdList={string.Join(',', accountIds)}&mapIdList={string.Join(',', mapIds)}",
            NadeoAPIJsonContext.Default.MapRecordArray, cancellationToken);
    }

    public virtual async Task<MapRecord> GetMapRecordByIdAsync(Guid mapRecordId, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"mapRecords/{mapRecordId}", NadeoAPIJsonContext.Default.MapRecord, cancellationToken);
    }

    public virtual async Task<Account[]> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/displayNames/?accountIdList={string.Join(',', accountIds)}",
            NadeoAPIJsonContext.Default.AccountArray, cancellationToken);
    }

    public async Task<Account[]> GetAccountDisplayNamesAsync(params Guid[] accountIds)
    {
        return await GetAccountDisplayNamesAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<PlayerZone[]> GetPlayerZonesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/zones/?accountIdList={string.Join(',', accountIds)}",
            NadeoAPIJsonContext.Default.PlayerZoneArray, cancellationToken);
    }

    public async Task<PlayerZone[]> GetPlayerZonesAsync(params Guid[] accountIds)
    {
        return await GetPlayerZonesAsync(accountIds, CancellationToken.None);
    }

    public virtual async Task<Dictionary<string, ApiRoute>> GetApiRoutesAsync(ApiUsage usage, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"api/routes?usage={usage}", NadeoAPIJsonContext.Default.DictionaryStringApiRoute, cancellationToken);
    }

    public virtual async Task<Zone[]> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync("zones", NadeoAPIJsonContext.Default.ZoneArray, cancellationToken);
    }

    public async Task<PlayerClubTag[]> GetPlayerClubTagsAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"accounts/clubTags/?accountIdList={string.Join(',', accountIds)}", NadeoAPIJsonContext.Default.PlayerClubTagArray, cancellationToken);
    }

    public async Task<PlayerClubTag[]> GetPlayerClubTagsAsync(params Guid[] accountIds)
    {
        return await GetPlayerClubTagsAsync(accountIds, CancellationToken.None);
    }
}
