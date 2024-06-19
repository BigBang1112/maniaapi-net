using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubActivityEdition
{
    [JsonConverter(typeof(BoolNumberConverter))]
    public bool Featured { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool Public { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool Active { get; init; }

    public int Position { get; init; }
}
