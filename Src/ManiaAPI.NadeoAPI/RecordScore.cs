using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.NadeoAPI;

public sealed record RecordScore(
    [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 Time,
    int Score,
    [property: JsonConverter(typeof(NullableIntConverter))] int? RespawnCount)
{
    
}
