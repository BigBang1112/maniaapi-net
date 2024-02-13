using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Leaderboard(Record[] Tops, [property: JsonPropertyName("playercount")] int PlayerCount);
