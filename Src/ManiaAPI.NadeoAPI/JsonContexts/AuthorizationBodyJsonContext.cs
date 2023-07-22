using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.JsonContexts;

[JsonSerializable(typeof(AuthorizationBody))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
partial class AuthorizationBodyJsonContext : JsonSerializerContext
{
}
