using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ChallengeLeaderboard(int ChallengeId, int Cardinal, string ScoreUnit, ImmutableList<ChallengeLeaderboardResult> Results);
