using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubMapReviewRoom(int ActivityId,
                                       string MapReviewUid,
                                       int TimeLimit,
                                       bool Scalable,
                                       bool AllowVoteSkipMap,
                                       bool SubmissionLimitation,
                                       int MaxPlayer,
                                       bool Public,
                                       int PopularityLevel,
                                       int SubmittedMapCount,
                                       int Id,
                                       int ClubId,
                                       string ClubName,
                                       string Name,
                                       [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                                       Guid CreatorAccountId,
                                       Guid LatestEditorAccountId,
                                       [property: JsonPropertyName("game2webUrl")] string Game2WebUrl,
                                       string MediaUrl,
                                       string MediaUrlPngLarge,
                                       string MediaUrlPngMedium,
                                       string MediaUrlPngSmall,
                                       string MediaUrlDds,
                                       string MediaTheme);