using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

[JsonSerializable(typeof(ManiaPlanetIngameAuthResponse))]
[JsonSerializable(typeof(IngameTitle))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ManiaPlanetIngameAPIJsonContext : JsonSerializerContext;