using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingHeartbeatStatus([property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? BanEndDate,
                                                string Status,
                                                string? MatchLiveId,
                                                int? MatchmakingWaitingTime,
                                                [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? CreationDate);
