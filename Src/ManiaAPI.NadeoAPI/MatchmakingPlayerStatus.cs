namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingPlayerStatus(MatchmakingHeartbeatStatus? CurrentHeartbeat,
                                             string? Penalty,
                                             Guid? CurrentDivision,
                                             int? CurrentProgression,
                                             string MatchmakingStatus,
                                             MatchmakingInactivityInfo Inactivity,
                                             int? MatchGenerationTimer);
