using ManiaAPI.TrackmaniaWS.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Player in a leaderboard.
/// </summary>
/// <param name="Rank">Rank in the zone of the leaderboard.</param>
/// <param name="Points">Points, in the unit of the leaderboard.</param>
/// <param name="Player">The player.</param>
public sealed record PlayerLeaderboardEntry([property: JsonConverter(typeof(StringInt32Converter))] int Rank,
                                            [property: JsonConverter(typeof(StringDecimalConverter))] decimal Points,
                                            Player Player);
