namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// A player's record on a map in a channel leaderboard.
/// </summary>
/// <param name="Record">The player's record, or null when the player has no record on the map.</param>
/// <param name="RecordCount">Number of records on the map.</param>
public sealed record ChannelPlayerRecord(ChannelRecord? Record, int RecordCount);
