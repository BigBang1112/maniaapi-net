using System.Text.Json.Serialization;
using ManiaAPI.NadeoAPI.Converters;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.NadeoAPI;

public sealed record Record(Guid AccountId, Guid ZoneId, string ZoneName, int Position, [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 Score, [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset Timestamp)
{
    public override string ToString()
    {
        return $"{Position}) {Score} by {AccountId}";
    }
}