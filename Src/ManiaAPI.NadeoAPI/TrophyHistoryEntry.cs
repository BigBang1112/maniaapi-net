namespace ManiaAPI.NadeoAPI;

public sealed record TrophyHistoryEntry(Guid AccountId,
                                         int T1Count,
                                         int T2Count,
                                         int T3Count,
                                         int T4Count,
                                         int T5Count,
                                         int T6Count,
                                         int T7Count,
                                         int T8Count,
                                         int T9Count,
                                         DateTimeOffset Timestamp,
                                         TrophyAchievementInfo TrophyAchievementInfo,
                                         TrophyGainDetails TrophyGainDetails);
