using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record CampaignRankingZone(Guid ZoneId, string ZoneName, ImmutableList<CampaignRankingEntry> Top);
