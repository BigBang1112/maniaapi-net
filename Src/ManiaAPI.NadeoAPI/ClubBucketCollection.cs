using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubBucketCollection(ImmutableList<ClubBucket> ClubBucketList,
                                          int MaxPage,
                                          int ItemCount);
