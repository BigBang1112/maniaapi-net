using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubBucketCollection(ImmutableArray<ClubBucket> ClubBucketList,
                                          int MaxPage,
                                          int ItemCount);
