namespace ManiaAPI.NadeoAPI;

public sealed record ChallengeLeaderboardResult(long Points, Guid Player, long Score, int Rank, string Zone);
