using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record ClubRoom(int Id,
                              [property: JsonPropertyName("clubid")] int ClubId,
                              [property: JsonPropertyName("clubname")] string ClubName,
                              string Name,
                              bool Nadeo,
                              string Login,
                              [property: JsonPropertyName("playercount")] int PlayerCount,
                              [property: JsonPropertyName("playermax")] int PlayerMax,
                              string Region,
                              string Script,
                              [property: JsonPropertyName("scriptsettings"), JsonIgnore] ImmutableDictionary<string, ScriptSetting> ScriptSettings, // bugged atm
                              [property: JsonPropertyName("mediaurl")] string MediaUrl,
                              ImmutableArray<Map> Maps);
