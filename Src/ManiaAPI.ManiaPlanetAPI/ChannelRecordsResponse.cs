namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// A range of a leaderboard as returned by the Channel Record API, keyed by position in the response, starting at 1.
/// </summary>
internal sealed record ChannelRecordsResponse(Dictionary<int, ChannelRecord> Ranking);
