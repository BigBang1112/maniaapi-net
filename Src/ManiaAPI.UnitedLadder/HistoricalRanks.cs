using System.Collections.Immutable;

namespace ManiaAPI.UnitedLadder;

public sealed record HistoricalRanks(ImmutableList<HistoricalRankInfo> Solo,
                                     ImmutableDictionary<string, ImmutableList<HistoricalRankInfo>> Multiplayer);
