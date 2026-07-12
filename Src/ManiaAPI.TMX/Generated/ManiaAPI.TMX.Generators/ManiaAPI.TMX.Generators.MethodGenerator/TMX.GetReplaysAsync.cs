using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;

namespace ManiaAPI.TMX;

public partial class TMX
{
    public virtual partial async System.Threading.Tasks.Task<ManiaAPI.TMX.ItemCollection<ManiaAPI.TMX.ReplayItem>> GetReplaysAsync(ManiaAPI.TMX.TMX.GetReplaysParameters parameters, System.Threading.CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sb = new StringBuilder("api/replays");
        parameters.AppendQueryString(sb);

        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"api/replays\n{await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.ItemCollectionReplayItem, cancellationToken) ?? new();
    }
}
