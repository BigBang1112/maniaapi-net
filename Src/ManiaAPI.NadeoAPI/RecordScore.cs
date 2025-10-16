using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.NadeoAPI;

public sealed record RecordScore(
    [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 Time,
    int Score,
    [property: JsonConverter(typeof(NullableIntConverter))] int? RespawnCount)
{
    
}
