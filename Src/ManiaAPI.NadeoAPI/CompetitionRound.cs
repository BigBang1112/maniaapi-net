using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionRound(int Id,
                                      int Position,
                                      string Name,
                                      [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                                      [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate,
                                      [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? LockDate,
                                      string? Status,
                                      bool IsLocked,
                                      bool AutoNeedsMatches,
                                      string MatchScoreDirection,
                                      string LeaderboardComputeType,
                                      string? TeamLeaderboardComputeType,
                                      [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? DeletedOn,
                                      int NbMatches,
                                      int? QualifierChallengeId,
                                      int? TrainingChallengeId);
