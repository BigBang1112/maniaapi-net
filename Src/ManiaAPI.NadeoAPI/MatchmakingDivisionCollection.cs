using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingDivisionCollection(ImmutableList<MatchmakingDivision> Divisions);
