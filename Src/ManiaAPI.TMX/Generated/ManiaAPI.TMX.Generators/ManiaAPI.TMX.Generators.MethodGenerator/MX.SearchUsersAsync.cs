using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;

namespace ManiaAPI.TMX;

public partial class MX
{
    public virtual partial async System.Threading.Tasks.Task<ManiaAPI.TMX.ItemCollection<ManiaAPI.TMX.UserItemMX>> SearchUsersAsync(ManiaAPI.TMX.MX.SearchUsersParameters parameters, System.Threading.CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sb = new StringBuilder("api/users");
        parameters.AppendQueryString(sb);

        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"api/users\n{await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.ItemCollectionUserItemMX, cancellationToken) ?? new();
    }
}
