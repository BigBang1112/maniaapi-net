using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCampaignTopCollection(ImmutableList<CampaignRankingEntry> Top, int ClubId, string GroupUid, int Length);
