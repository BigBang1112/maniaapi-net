using System.Collections;

namespace ManiaAPI.TrackmaniaIO;

public class CampaignCollection : IReadOnlyCollection<CampaignItem>
{
    private readonly CampaignItem[] campaigns;

    public int Page { get; init; }

    public CampaignCollection(CampaignItem[] campaigns, int page = 0)
    {
        this.campaigns = campaigns;

        Page = page;
    }

    public CampaignItem this[int index]
    {
        get
        {
            return campaigns[index];
        }
    }

    public int Count => campaigns.Length;

    public IEnumerator<CampaignItem> GetEnumerator()
    {
        return ((IEnumerable<CampaignItem>)campaigns).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return campaigns.GetEnumerator();
    }
}
