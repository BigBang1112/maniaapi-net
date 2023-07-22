using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.JsonContexts;

[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
partial class AuthorizationResponseJsonContext : JsonSerializerContext
{
}
