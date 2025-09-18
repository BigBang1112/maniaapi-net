using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Leaderboard(ImmutableList<Record> Tops, [property: JsonPropertyName("playercount")] int PlayerCount);
