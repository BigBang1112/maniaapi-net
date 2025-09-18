using ManiaAPI.TrackmaniaIO.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record CompetitionRound(int Id,
                                      string Name,
                                      string Status,
                                      [property: JsonConverter(typeof(DateTimeOffsetUnixConverter)), JsonPropertyName("startdate")] DateTimeOffset StartDate,
                                      [property: JsonConverter(typeof(DateTimeOffsetUnixConverter)), JsonPropertyName("enddate")] DateTimeOffset EndDate,
                                      ImmutableList<CompetitionRoundMatch> Matches,
                                      ImmutableList<CompetitionRoundChallenge> Challenges);
