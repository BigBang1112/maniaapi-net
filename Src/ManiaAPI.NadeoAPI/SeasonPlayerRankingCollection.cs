namespace ManiaAPI.NadeoAPI;

public sealed record SeasonPlayerRankingCollection(string GroupUid,
                                                   string Sp,
                                                   SeasonPlayerRankingZone[] Zones);