using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record SeasonPlayerRankingCollection(string GroupUid,
                                                   string Sp,
                                                   ImmutableList<SeasonPlayerRankingZone> Zones);