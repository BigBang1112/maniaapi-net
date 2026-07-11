using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubRanking([property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                                 string ClubName,
                                 int Id,
                                 int? CampaignId,
                                 string Name,
                                 int ClubId,
                                 Guid LatestEditorAccountId,
                                 Guid CreatorAccountId,
                                 string UseCase);
