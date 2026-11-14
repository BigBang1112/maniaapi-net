using ManiaAPI.TrackmaniaWS.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Leaderboard of players on a challenge (map) in a zone.
/// </summary>
/// <param name="IdZone">ID of the zone.</param>
/// <param name="Path">Path of the zone.</param>
/// <param name="ChallengeUid">UID of the challenge.</param>
/// <param name="GameMode">Game mode, like <c>Race</c>.</param>
/// <param name="Unit">Unit of the points, like <c>Score</c>. In races, the score is the time in milliseconds.</param>
/// <param name="Players">Players.</param>
public sealed record ChallengeLeaderboard([property: JsonConverter(typeof(StringInt32Converter))] int IdZone,
                                          string Path,
                                          [property: JsonPropertyName("challengeUID")] string ChallengeUid,
                                          string GameMode,
                                          string Unit,
                                          ImmutableList<PlayerLeaderboardEntry> Players);
