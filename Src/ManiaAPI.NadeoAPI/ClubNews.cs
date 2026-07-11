using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubNews([property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                              string MediaUrlJpgLarge,
                              string MediaUrlJpgMedium,
                              string ClubName,
                              int Id,
                              string Name,
                              int ClubId,
                              string MediaUrl,
                              Guid LatestEditorAccountId,
                              string Body,
                              string MediaTheme,
                              Guid CreatorAccountId,
                              string Headline,
                              string MediaUrlJpgSmall,
                              string MediaUrlDds);
