using ManiaAPI.TrackmaniaWS.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Leaderboard of the child zones of a zone.
/// </summary>
/// <param name="IdZone">ID of the parent zone.</param>
/// <param name="Path">Path of the parent zone.</param>
/// <param name="Environment">Environment, like <c>Merge</c> for all environments together.</param>
/// <param name="Unit">Unit of the points, like <c>Ladder Points</c>.</param>
/// <param name="Zones">Child zones, best first.</param>
public sealed record ZoneLeaderboard([property: JsonConverter(typeof(StringInt32Converter))] int IdZone,
                                     string Path,
                                     string Environment,
                                     string Unit,
                                     ImmutableList<ZoneLeaderboardEntry> Zones);
