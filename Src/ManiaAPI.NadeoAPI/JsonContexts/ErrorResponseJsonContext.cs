using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.JsonContexts;

[JsonSerializable(typeof(ErrorResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
partial class ErrorResponseJsonContext : JsonSerializerContext
{
}
