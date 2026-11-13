namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// A player's record as returned by the Channel Record API. Only <see cref="Length"/> is set when the player has no record.
/// </summary>
internal sealed record ChannelPlayerRecordResponse(string? UserLogin, string? UserNickname, int? Rank, int? Score, int Length);
