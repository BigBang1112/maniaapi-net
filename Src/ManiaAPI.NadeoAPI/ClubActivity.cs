using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubActivity(int Id,
                                  string Name,
                                  string ActivityType,
                                  int ActivityId,
                                  int TargetActivityId,
                                  int CampaignId,
                                  int Position,
                                  bool Public,
                                  bool Active,
                                  int ExternalId,
                                  bool Featured,
                                  bool Password,
                                  int ItemsCount,
                                  int ClubId,
                                  [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EditionTimestamp,
                                  Guid CreatorAccountId,
                                  Guid LatestEditorAccountId,
                                  string MediaUrl,
                                  string MediaUrlPngLarge,
                                  string MediaUrlPngMedium,
                                  string MediaUrlPngSmall,
                                  string MediaUrlDds,
                                  string MediaTheme);