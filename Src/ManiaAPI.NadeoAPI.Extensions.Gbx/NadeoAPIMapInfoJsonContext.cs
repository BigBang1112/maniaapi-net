using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.JsonContexts;

[JsonSerializable(typeof(MapInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class NadeoAPIMapInfoJsonContext : JsonSerializerContext;
