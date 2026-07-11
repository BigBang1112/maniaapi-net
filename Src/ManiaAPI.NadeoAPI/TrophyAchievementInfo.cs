namespace ManiaAPI.NadeoAPI;

public sealed record TrophyAchievementInfo(Guid TrophyAchievementId,
                                            string TrophyAchievementType,
                                            string? TrophySoloMedalAchievementType = null,
                                            Guid? CompetitionId = null,
                                            string? CompetitionMatchInfo = null,
                                            string? CompetitionName = null,
                                            string? CompetitionStage = null,
                                            string? CompetitionStageStep = null,
                                            string? CompetitionType = null,
                                            Guid? ServerId = null);
