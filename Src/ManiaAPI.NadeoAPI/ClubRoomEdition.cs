using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubRoomEdition
{
    public string? Name { get; init; }
    public string? Region { get; init; }
    public int? MaxPlayersPerServer { get; init; }
    public string? Script { get; init; }
    public ImmutableList<ScriptSetting>? Settings { get; init; }
    public ImmutableList<string>? Maps { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? Scalable { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? ShufflePlaylist { get; init; }
}
