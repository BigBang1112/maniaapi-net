using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

[JsonSerializable(typeof(Player))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class TrackmaniaWSJsonContext : JsonSerializerContext { }