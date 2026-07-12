using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;

namespace ManiaAPI.TMX;

public partial class MX
{
    public virtual partial async System.Threading.Tasks.Task<ManiaAPI.TMX.SearchOrderItem[]> GetUserSearchOrdersAsync(System.Threading.CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await Client.GetAsync("api/meta/userorders", cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"api/meta/userorders\n{await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.SearchOrderItemArray, cancellationToken) ?? [];
    }
}
