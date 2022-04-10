namespace ManiaAPI.TrackmaniaIO;

public record CustomCampaignItem : CampaignItem, ICustomCampaignItem
{
    public int ClubId { get; init; }

    public CustomCampaignItem(int id, int clubId, string name, DateTimeOffset timestamp, int mapCount) : base(id, name, timestamp, mapCount)
    {
        ClubId = clubId;
    }

    public override async Task<ICampaign> GetDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await TrackmaniaIO.GetCustomCampaignAsync(ClubId, Id, cancellationToken);
    }
}
