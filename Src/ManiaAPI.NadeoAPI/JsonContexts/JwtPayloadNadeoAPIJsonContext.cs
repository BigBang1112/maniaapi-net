using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.JsonContexts;

[JsonSerializable(typeof(JwtPayloadNadeoAPI))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
partial class JwtPayloadNadeoAPIJsonContext : JsonSerializerContext
{
}
