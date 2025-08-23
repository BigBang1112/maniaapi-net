using System.Text.Json.Serialization;

namespace ManiaAPI.UnitedLadder;

[JsonSerializable(typeof(Player))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class UnitedLadderJsonContext : JsonSerializerContext { }