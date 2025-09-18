using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubActivityCollection(ImmutableList<ClubActivity> ActivityList,
                                            int MaxPage,
                                            int ItemCount);
