using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCollection(ImmutableArray<Club> ClubList,
                                    int MaxPage,
                                    int ItemCount);
