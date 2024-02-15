using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Leaderboard(ImmutableArray<Record> Tops, [property: JsonPropertyName("playercount")] int PlayerCount);
