using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.UnitedLadder;

public sealed record HistoricalSummaryData(string Path, HistoricalRanks Ranks, int Stars, bool United,
                                           [property: JsonPropertyName("id_zone")] int IdZone,
                                           string Nickname,
                                           [property: JsonPropertyName("tag_count")] int TagCount,
                                           string Environment, double Skillpoints,
                                           [property: JsonPropertyName("zone_update")] long ZoneUpdate,
                                           [property: JsonPropertyName("multiplayer_points")] ImmutableDictionary<string, double> MultiplayerPoints);
