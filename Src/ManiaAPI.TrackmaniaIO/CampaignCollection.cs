using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record CampaignCollection(ImmutableList<CampaignItem> Campaigns, int Page, int PageCount);