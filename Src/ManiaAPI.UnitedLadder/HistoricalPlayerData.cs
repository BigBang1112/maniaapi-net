using System.Text.Json.Serialization;

namespace ManiaAPI.UnitedLadder;

public sealed record HistoricalPlayerData([property: JsonPropertyName("data_date")] DateTimeOffset DataDate,
                                          [property: JsonPropertyName("full_data")] Player? FullData,
                                          [property: JsonPropertyName("summary_data")] HistoricalSummaryData? SummaryData,
                                          [property: JsonPropertyName("is_permanent")] bool IsPermanent,
                                          [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
                                          [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);
