using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;

namespace ManiaAPI.TMX;

public partial class MX
{
    public virtual partial async System.Threading.Tasks.Task<ManiaAPI.TMX.TagInfo[]> GetTagsAsync(System.Threading.CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await Client.GetAsync("api/meta/tags", cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"api/meta/tags\n{await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.TagInfoArray, cancellationToken) ?? [];
    }
}
