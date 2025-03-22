using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubActivityCollection(ImmutableArray<ClubActivity> ActivityList,
                                            int MaxPage,
                                            int ItemCount);
