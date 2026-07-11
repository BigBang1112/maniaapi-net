using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record TrophyRankingCollection(ImmutableList<TrophyPlayerRanking> Rankings, int Length);
