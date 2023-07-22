using ManiaAPI.NadeoAPI.JsonContexts;

namespace ManiaAPI.NadeoAPI;

public interface INadeoServices : INadeoAPI
{
    Task<string> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default);
    Task<MapRecord[]> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default);
}

public class NadeoServices : NadeoAPI, INadeoServices
{
    public override string Audience => nameof(NadeoServices);

    public NadeoServices(HttpClient client, bool automaticallyAuthorize = true) : base(client, automaticallyAuthorize)
    {
        client.BaseAddress = new Uri("https://prod.trackmania.core.nadeo.online/");
    }

    public NadeoServices(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize)
    {
    }

    public async Task<MapRecord[]> GetMapRecordsAsync(IEnumerable<Guid> accountIds, IEnumerable<Guid> mapIds, CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync($"mapRecords/?accountIdList={string.Join(',', accountIds)}&mapIdList={string.Join(',', mapIds)}",
            MapRecordArrayJsonContext.Default.MapRecordArray, cancellationToken);
    }

    public async Task<string> GetAccountDisplayNamesAsync(IEnumerable<Guid> accountIds, CancellationToken cancellationToken = default)
    {
        return await GetAsync($"accounts/displayNames/?accountIdList={string.Join(',', accountIds)}", cancellationToken);
    }
}
