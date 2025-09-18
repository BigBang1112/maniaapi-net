using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionCollection(ImmutableList<ClubCompetition> ClubCompetitionList,
                                               int MaxPage,
                                               int ItemCount);