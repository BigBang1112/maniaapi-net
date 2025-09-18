using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCollection(ImmutableList<Club> ClubList,
                                    int MaxPage,
                                    int ItemCount);
