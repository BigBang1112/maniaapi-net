using ManiaAPI.NadeoAPI.JsonContexts;

namespace ManiaAPI.NadeoAPI;

public interface INadeoClubServices : INadeoAPI
{
    Task<CupOfTheDay?> GetCurrentCupOfTheDayAsync(CancellationToken cancellationToken = default);
}

public class NadeoClubServices : NadeoAPI, INadeoClubServices
{
    public override string Audience => nameof(NadeoLiveServices);
    public override string BaseAddress => "https://meet.trackmania.nadeo.club/api";

    public NadeoClubServices(HttpClient client, bool automaticallyAuthorize = true) : base(client, automaticallyAuthorize)
    {
    }

    public NadeoClubServices(bool automaticallyAuthorize = true) : this(new HttpClient(), automaticallyAuthorize)
    {
    }

    public virtual async Task<CupOfTheDay?> GetCurrentCupOfTheDayAsync(CancellationToken cancellationToken = default)
    {
        return await GetNullableJsonAsync("cup-of-the-day/current", NadeoAPIJsonContext.Default.CupOfTheDay, cancellationToken);
    }
}
