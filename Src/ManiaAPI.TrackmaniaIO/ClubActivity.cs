using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubActivity(int Id,
                                  string Name,
                                  string Type,
                                  [property: JsonPropertyName("activityid")] int ActivityId,
                                  [property: JsonPropertyName("targetactivityid")] int TargetActivityId,
                                  [property: JsonPropertyName("campaignid")] int? CampaignId,
                                  int Position,
                                  bool Public,
                                  string Media,
                                  [property: JsonPropertyName("externalid")] int? ExternalId,
                                  bool Password);