using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionCollection(ImmutableArray<ClubCompetition> ClubCompetitionList,
                                               int MaxPage,
                                               int ItemCount);