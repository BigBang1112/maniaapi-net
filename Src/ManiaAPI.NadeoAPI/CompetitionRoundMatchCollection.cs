using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionRoundMatchCollection(ImmutableList<CompetitionRoundMatchSummary> Matches);
