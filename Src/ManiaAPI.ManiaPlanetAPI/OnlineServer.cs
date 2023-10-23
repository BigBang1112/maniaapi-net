using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

public sealed record OnlineServer(
    string Name,
    string Description,
    string Login,
    string Title,
    [property: JsonPropertyName("ladder_limit_min")] int LadderLimitMin,
    [property: JsonPropertyName("ladder_limit_max")] int LadderLimitMax,
    [property: JsonPropertyName("average_ladder_points")] int AverageLadderPoints,
    [property: JsonPropertyName("player_count")] int PlayerCount,
    [property: JsonPropertyName("player_max")] int PlayerMax,
    [property: JsonPropertyName("spectator_count")] int SpectatorCount,
    string Zone,
    [property: JsonPropertyName("is_private")] bool IsPrivate,
    [property: JsonPropertyName("is_spectator_private")] bool IsSpectatorPrivate,
    [property: JsonPropertyName("is_lobby")] bool IsLobby,
    [property: JsonPropertyName("level_class_1")] int LevelClass1,
    [property: JsonPropertyName("level_class_2")] int LevelClass2,
    [property: JsonPropertyName("level_class_3")] int LevelClass3,
    [property: JsonPropertyName("level_class_4")] int LevelClass4,
    [property: JsonPropertyName("level_class_5")] int LevelClass5,
    [property: JsonPropertyName("script_name")] string ScriptName,
    [property: JsonPropertyName("game_mode")] int GameMode,
    [property: JsonPropertyName("relay_of")] string RelayOf,
    string Environment);