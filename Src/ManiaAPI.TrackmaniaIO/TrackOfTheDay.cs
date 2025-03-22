using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record TrackOfTheDay(int CampaignId,
                                   Map Map,
                                   [property: JsonPropertyName("weekday")] int WeekDay,
                                   [property: JsonPropertyName("monthday")] int MonthDay,
                                   [property: JsonPropertyName("leaderboarduid")] Guid LeaderboardUid);