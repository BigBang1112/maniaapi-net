using ManiaAPI.TrackmaniaWS.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Leaderboard of players in a zone.
/// </summary>
/// <param name="IdZone">ID of the zone.</param>
/// <param name="Path">Path of the zone.</param>
/// <param name="Environment">Environment, like <c>Merge</c> for all environments together.</param>
/// <param name="Unit">Unit of the points, like <c>Ladder Points</c> or <c>Skill Points</c>.</param>
/// <param name="Players">Players.</param>
public sealed record PlayerLeaderboard([property: JsonConverter(typeof(StringInt32Converter))] int IdZone,
                                       string Path,
                                       string Environment,
                                       string Unit,
                                       ImmutableList<PlayerLeaderboardEntry> Players);
