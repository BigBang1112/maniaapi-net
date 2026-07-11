namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingInactivityInfo(bool InactivityPenaltyEnabled, int ImmunityDays, int Penalty);
