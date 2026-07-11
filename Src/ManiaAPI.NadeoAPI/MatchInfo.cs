using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record MatchInfo(int Id,
                               string LiveId,
                               string Name,
                               [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                               [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate,
                               string Status,
                               string ParticipantType,
                               string? JoinLink,
                               string ServerStatus,
                               string? Manialink,
                               MatchPublicConfig? PublicConfig);
