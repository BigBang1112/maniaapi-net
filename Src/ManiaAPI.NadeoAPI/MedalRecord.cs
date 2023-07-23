using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.NadeoAPI;

public record MedalRecord(string Medal,
                          Guid AccountId,
                          Guid ZoneId,
                          string ZoneName,
                          [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 Score);