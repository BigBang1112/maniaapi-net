using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record TrackOfTheDay(int CampaignId,
                                   string MapUid,
                                   int Day,
                                   int MonthDay,
                                   [property: JsonConverter(typeof(NullableGuidConverter))] Guid? SeasonUid,
                                   string? LeaderboardGroup,
                                   [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartTimestamp,
                                   [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndTimestamp,
                                   int RelativeStart,
                                   int RelativeEnd);