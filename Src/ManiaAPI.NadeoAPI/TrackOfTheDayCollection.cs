using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record TrackOfTheDayCollection(TrackOfTheDayMonth[] MonthList,
                                             [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset NextRequestTimestamp,
                                             int RelativeNextRequest);