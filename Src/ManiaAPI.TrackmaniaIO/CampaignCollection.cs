using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record CampaignCollection(ImmutableArray<CampaignItem> Campaigns, int Page, int PageCount);