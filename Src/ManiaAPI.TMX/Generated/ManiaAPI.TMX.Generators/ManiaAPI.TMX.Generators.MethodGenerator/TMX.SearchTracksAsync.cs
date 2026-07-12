using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;

namespace ManiaAPI.TMX;

public partial class TMX
{
    public virtual partial async System.Threading.Tasks.Task<ManiaAPI.TMX.ItemCollection<ManiaAPI.TMX.TrackItem>> SearchTracksAsync(ManiaAPI.TMX.TMX.SearchTracksParameters parameters, System.Threading.CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sb = new StringBuilder("api/tracks");
        parameters.AppendQueryString(sb);

        using var response = await Client.GetAsync(sb.ToString(), cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.WriteLine($"api/tracks\n{await response.Content.ReadAsStringAsync(cancellationToken)}");

        return await response.Content.ReadFromJsonAsync(TMXJsonContext.Default.ItemCollectionTrackItem, cancellationToken) ?? new();
    }
}
