namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionLeaderboardEntry(Guid Participant, int Rank, long Score, string Zone);
