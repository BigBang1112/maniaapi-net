using System.Text.Json.Serialization;

namespace ManiaAPI.UnitedLadder;

public sealed record HistoricalRankInfo(int Rank, int Zone,
                                        [property: JsonPropertyName("zone_path")] string ZonePath);
