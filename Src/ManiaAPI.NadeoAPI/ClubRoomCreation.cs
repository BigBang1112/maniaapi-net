using ManiaAPI.NadeoAPI.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubRoomCreation
{
    public required string Name { get; init; }
    public required string Region { get; init; }
    public required int MaxPlayersPerServer { get; init; }
    public required string Script { get; init; }
    public ImmutableList<ScriptSetting>? Settings { get; init; }
    public ImmutableList<string>? Maps { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? Scalable { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? Password { get; init; }

    [JsonConverter(typeof(BoolNumberConverter))]
    public bool? ShufflePlaylist { get; init; }

    public int? FolderId { get; init; }
}
