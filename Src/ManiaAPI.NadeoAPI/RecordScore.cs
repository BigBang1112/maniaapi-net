using ManiaAPI.Base.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.NadeoAPI;

public record RecordScore(TimeInt32 Time, int Score,
    [property: JsonConverter(typeof(NullableIntConverter))] int? RespawnCount)
{
    
}
