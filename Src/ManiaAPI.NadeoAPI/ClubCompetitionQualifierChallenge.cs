using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionQualifierChallenge(int Id,
                                                        string Name,
                                                        [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset StartDate,
                                                        [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset EndDate,
                                                        bool IsCompleted,
                                                        ImmutableList<string>? Servers);
