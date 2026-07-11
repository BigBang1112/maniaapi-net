using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionMatchResults(string MatchLiveId,
                                             int RoundPosition,
                                             ImmutableList<CompetitionMatchPlayerResult> Results,
                                             string ScoreUnit,
                                             ImmutableList<CompetitionMatchTeamResult> Teams);
