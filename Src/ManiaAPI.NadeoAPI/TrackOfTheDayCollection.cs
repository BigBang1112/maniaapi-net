using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record TrackOfTheDayCollection(ImmutableArray<TrackOfTheDayMonth> MonthList,
                                             [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset NextRequestTimestamp,
                                             int RelativeNextRequest);