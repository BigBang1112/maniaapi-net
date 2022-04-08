namespace ManiaAPI.NadeoAPI;

public class NadeoServices : NadeoAPI
{
    public NadeoServices(bool automaticallyAuthorize = true) : base("https://prod.trackmania.core.nadeo.online/", automaticallyAuthorize)
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
