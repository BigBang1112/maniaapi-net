using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCampaign(string ClubDecalUrl,
                                  int CampaignId,
                                  int ActivityId,
                                  Campaign Campaign,
                                  int PopularityLevel,
                                  [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset PublicationTimestamp,
                                  [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                                  Guid CreatorAccountId,
                                  Guid LatestEditorAccountId,
                                  int Id,
                                  int ClubId,
                                  string ClubName,
                                  string Name,
                                  int MapsCount,
                                  string MediaUrl,
                                  string MediaUrlPngLarge,
                                  string MediaUrlPngMedium,
                                  string MediaUrlPngSmall,
                                  string MediaUrlDds,
                                  string MediaTheme);