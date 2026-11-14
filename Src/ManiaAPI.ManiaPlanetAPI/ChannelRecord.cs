namespace ManiaAPI.ManiaPlanetAPI;

/// <summary>
/// A record of a player on a map in a channel leaderboard.
/// </summary>
/// <param name="UserLogin">Login of the player.</param>
/// <param name="UserNickname">Nickname of the player, with formatting codes.</param>
/// <param name="Rank">Rank of the record, starting at 1.</param>
/// <param name="Score">Score of the record, usually a time in milliseconds where lower is better.</param>
public sealed record ChannelRecord(string UserLogin, string UserNickname, int Rank, int Score);
