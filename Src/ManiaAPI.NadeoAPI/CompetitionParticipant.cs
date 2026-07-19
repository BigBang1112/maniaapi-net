using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionParticipant(Guid Participant,
                                            string Zone,
                                            [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset Registration,
                                            int? Seed,
                                            [property: JsonConverter(typeof(NullableGuidConverter))] Guid? AddedBy,
                                            string? Team,
                                            [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? CheckInDate,
                                            string? GroupId,
                                            int? SkillLevel);
