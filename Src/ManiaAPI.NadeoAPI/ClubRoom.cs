using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubRoom(int Id,
                              int ClubId,
                              string ClubName,
                              bool Nadeo,
                              int? RoomId,
                              int? CampaignId,
                              string? PlayerServerLogin,
                              int ActivityId,
                              string Name,
                              Room Room,
                              int PopularityLevel,
                              [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                              Guid CreatorAccountId,
                              Guid LatestEditorAccountId,
                              bool Password,
                              string MediaUrl,
                              string MediaUrlPngLarge,
                              string MediaUrlPngMedium,
                              string MediaUrlPngSmall,
                              string MediaUrlDds,
                              string MediaTheme);
