using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.Extensions.Gbx;

[JsonSerializable(typeof(MapInfoSubmit))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class NadeoAPIMapInfoJsonContext : JsonSerializerContext;
