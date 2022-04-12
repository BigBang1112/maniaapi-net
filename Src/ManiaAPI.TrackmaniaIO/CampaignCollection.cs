using System.Collections;

namespace ManiaAPI.TrackmaniaIO;

public class CampaignCollection : IReadOnlyCollection<ICampaignItem>
{
    private readonly ICampaignItem[] campaigns;

    public int Page { get; init; }

    public CampaignCollection(ICampaignItem[] campaigns, int page = 0)
    {
        this.campaigns = campaigns;

        Page = page;
    }

    public ICampaignItem this[int index]
    {
        get
        {
            return campaigns[index];
        }
    }

    public int Count => campaigns.Length;

    public IEnumerator<ICampaignItem> GetEnumerator()
    {
        return ((IEnumerable<ICampaignItem>)campaigns).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return campaigns.GetEnumerator();
    }
}
