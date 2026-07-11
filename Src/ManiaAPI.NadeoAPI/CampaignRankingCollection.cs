using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record CampaignRankingCollection(string GroupUid, ImmutableList<CampaignRankingZone> Tops);
