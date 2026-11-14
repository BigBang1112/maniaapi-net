using ManiaAPI.TrackmaniaWS.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

/// <summary>
/// Rank of a player in one of the zones they belong to.
/// </summary>
/// <param name="IdZone">ID of the zone.</param>
/// <param name="Path">Path of the zone.</param>
/// <param name="Rank">Rank in the zone.</param>
public sealed record PlayerZoneRank([property: JsonConverter(typeof(StringInt32Converter))] int IdZone,
                                    string Path,
                                    [property: JsonConverter(typeof(StringInt32Converter))] int Rank);
