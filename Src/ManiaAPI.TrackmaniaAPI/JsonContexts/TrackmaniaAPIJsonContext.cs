using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaAPI.JsonContexts;

[JsonSerializable(typeof(JwtPayloadTrackmaniaAPI))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Dictionary<Guid, string>))]
[JsonSerializable(typeof(Dictionary<string, Guid>))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
sealed partial class TrackmaniaAPIJsonContext : JsonSerializerContext { }