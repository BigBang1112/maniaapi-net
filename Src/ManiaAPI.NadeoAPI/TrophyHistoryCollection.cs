using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record TrophyHistoryCollection(int Count, ImmutableList<TrophyHistoryEntry> Data, int Offset, int TotalCount);
