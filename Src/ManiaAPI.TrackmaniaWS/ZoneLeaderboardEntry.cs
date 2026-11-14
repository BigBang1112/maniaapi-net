using ManiaAPI.TrackmaniaWS.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Zone in a leaderboard.
/// </summary>
/// <param name="Points">Points, in the unit of the leaderboard.</param>
/// <param name="Zone">The zone.</param>
public sealed record ZoneLeaderboardEntry([property: JsonConverter(typeof(StringDecimalConverter))] decimal Points,
                                          Zone Zone);
