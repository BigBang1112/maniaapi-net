using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record TrackOfTheDayInfo([property: JsonConverter(typeof(NullableIntConverter))] int? OfficialYear,
                                       [property: JsonConverter(typeof(NullableIntConverter))] int? Season,
                                       ImmutableArray<string> OfficialMaps,
                                       [property: JsonConverter(typeof(NullableIntConverter))] int? TotdYear,
                                       [property: JsonConverter(typeof(NullableIntConverter))] int? Week,
                                       ImmutableArray<string> TotdMaps);