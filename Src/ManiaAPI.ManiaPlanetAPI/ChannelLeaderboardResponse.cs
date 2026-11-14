namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// A leaderboard as returned by the Channel Record API. <see cref="Ranking"/> is keyed by position in the response, starting at 1,
/// and <see cref="ListPlayer"/> maps requested logins to ranks; it's missing when none of them has a record.
/// </summary>
internal sealed record ChannelLeaderboardResponse(Dictionary<int, ChannelRecord> Ranking, int Length, Dictionary<string, int>? ListPlayer);
