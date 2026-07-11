using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingRankingCollection(int MatchmakingId, long Cardinal, ImmutableList<MatchmakingPlayerRank> Results);
