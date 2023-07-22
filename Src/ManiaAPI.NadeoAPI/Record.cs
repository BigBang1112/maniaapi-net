using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.NadeoAPI;

public sealed record Record(Guid AccountId, Guid ZoneId, string ZoneName, int Position, [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 Score)
{
    public override string ToString()
    {
        return $"{Position}) {Score} by {AccountId}";
    }
}