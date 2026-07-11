using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionListItemCollection(ImmutableList<ClubCompetitionListItem> ClubCompetitions, int ClubCompetitionsCount);
