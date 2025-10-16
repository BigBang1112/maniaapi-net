using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.NadeoAPI;

public sealed record MedalRecord(string Medal,
                                 Guid AccountId,
                                 Guid ZoneId,
                                 string ZoneName,
                                 [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 Score);