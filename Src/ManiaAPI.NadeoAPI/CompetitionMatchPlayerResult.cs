using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionMatchPlayerResult(Guid Participant,
                                                   int? Rank,
                                                   int? Score,
                                                   string Zone,
                                                   [property: JsonConverter(typeof(NullableGuidConverter))] Guid? Team);
