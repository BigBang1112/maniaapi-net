namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingDivision(Guid Id,
                                         int Position,
                                         string DisplayRuleType,
                                         int? DisplayRuleMinimumPoints,
                                         int? DisplayRuleMaximumPoints,
                                         int? DisplayRuleMinimumRank,
                                         int? DisplayRuleMinimumVictories,
                                         int? DisplayRuleMaximumVictories);
