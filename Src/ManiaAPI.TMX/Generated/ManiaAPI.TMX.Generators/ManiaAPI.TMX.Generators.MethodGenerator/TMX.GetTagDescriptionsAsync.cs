using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;

namespace ManiaAPI.TMX;

public partial class TMX
{
    public virtual partial async System.Threading.Tasks.Task<ManiaAPI.TMX.TagDescription[]> GetTagDescriptionsAsync(System.Threading.CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await Client.GetAsync("api/meta/tagdescriptions", cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"api/meta/tagdescriptions\n{await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.TagDescriptionArray, cancellationToken) ?? [];
    }
}
