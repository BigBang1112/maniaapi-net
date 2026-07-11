using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionRoundSummary(int Id,
                                                 string Name,
                                                 int Position,
                                                 string? Status,
                                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate,
                                                 int NbMatches,
                                                 ClubCompetitionQualifierChallenge? QualifierChallenge);
