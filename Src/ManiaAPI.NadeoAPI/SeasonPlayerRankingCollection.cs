using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record SeasonPlayerRankingCollection(string GroupUid,
                                                   string Sp,
                                                   ImmutableArray<SeasonPlayerRankingZone> Zones);