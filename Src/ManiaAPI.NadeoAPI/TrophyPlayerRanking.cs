using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record TrophyPlayerRanking(long CountPoint, ImmutableList<SeasonPlayerRankingZone> Zones, Guid AccountId, int Echelon);
