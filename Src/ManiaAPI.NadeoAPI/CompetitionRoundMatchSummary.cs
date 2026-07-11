using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record CompetitionRoundMatchSummary(int Id,
                                                   string Name,
                                                   string ClubMatchLiveId,
                                                   int Position,
                                                   bool IsCompleted,
                                                   ImmutableList<string> Tags,
                                                   [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))] DateTimeOffset? DeletedOn);
