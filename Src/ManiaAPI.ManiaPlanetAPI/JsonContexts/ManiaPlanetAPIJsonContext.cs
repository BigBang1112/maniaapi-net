using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI.JsonContexts;

[JsonSerializable(typeof(JwtPayloadManiaPlanetAPI))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(Title[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
sealed partial class ManiaPlanetAPIJsonContext : JsonSerializerContext { }