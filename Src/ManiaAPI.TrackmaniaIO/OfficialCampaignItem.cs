﻿namespace ManiaAPI.TrackmaniaIO;

public record OfficialCampaignItem : CampaignItem, IOfficialCampaignItem
{
    public OfficialCampaignItem(int id, string name, DateTimeOffset timestamp, int mapCount) : base(id, name, timestamp, mapCount)
    {

    }

    public override async Task<ICampaign> GetDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await TrackmaniaIO.GetSeasonalCampaignAsync(Id, cancellationToken);
    }
}
