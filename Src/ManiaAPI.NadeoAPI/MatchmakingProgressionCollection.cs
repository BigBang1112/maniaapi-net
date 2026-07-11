using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingProgressionCollection(ImmutableList<MatchmakingPlayerProgression> Progressions);
