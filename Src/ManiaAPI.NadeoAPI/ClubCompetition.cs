using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetition(int CompetitionId,
                                     string MediaUrl,
                                     string MediaTheme,
                                     int PopularityLevel,
                                     int PopularityValue,
                                     int Id,
                                     int ClubId,
                                     string ClubName,
                                     string Name,
                                     [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                                     Guid CreatorAccountId,
                                     Guid LatestEditorAccountId);