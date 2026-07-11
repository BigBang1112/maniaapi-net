namespace ManiaAPI.NadeoAPI;

public sealed record CampaignRankingEntry(Guid AccountId, Guid ZoneId, string ZoneName, int Position, string Sp);
