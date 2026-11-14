using ManiaAPI.TrackmaniaWS.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Ranking of a player, with their rank in each zone they belong to.
/// </summary>
/// <param name="Environment">Environment, like <c>Merge</c> for all environments together.</param>
/// <param name="Unit">Unit of <paramref name="Points"/>, like <c>Ladder Points</c> or <c>Skill Points</c>.</param>
/// <param name="Points">Points of the player.</param>
/// <param name="Ranks">Rank of the player in each zone, from the world down.</param>
public sealed record PlayerRanking(string Environment,
                                   string Unit,
                                   [property: JsonConverter(typeof(StringDecimalConverter))] decimal Points,
                                   ImmutableList<PlayerZoneRank> Ranks);
