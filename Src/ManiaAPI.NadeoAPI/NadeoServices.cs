namespace ManiaAPI.NadeoAPI;

public interface INadeoServices : INadeoAPI
{
    Task<string> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<MapRecord[]> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
}

public class NadeoServices : NadeoAPI, INadeoServices
{
    private const string BaseUrl = "https://prod.trackmania.core.nadeo.online/";

    public NadeoServices(bool automaticallyAuthorize = true) : base(BaseUrl, automaticallyAuthorize)
    {

    }

    public NadeoServices(HttpClientHandler handler, bool automaticallyAuthorize = true) : base(handler, BaseUrl, automaticallyAuthorize)
    {

    }

    public async Task<MapRecord[]> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<MapRecord[]>($"mapRecords/?accountIdList={string.Join(',', accountIds)}&mapIdList={string.Join(',', mapIds)}", cancellationToken);
    }

    public async Task<string> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetApiAsync<string>($"accounts/displayNames/?accountIdList={string.Join(',', accountIds)}", cancellationToken);
    }
}
