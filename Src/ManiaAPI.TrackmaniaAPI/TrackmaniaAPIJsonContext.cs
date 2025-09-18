using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaAPI;

[JsonSerializable(typeof(JwtPayloadTrackmaniaAPI))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(ImmutableDictionary<Guid, string>))]
[JsonSerializable(typeof(ImmutableDictionary<string, Guid>))]
[JsonSerializable(typeof(ImmutableList<MapRecord>))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class TrackmaniaAPIJsonContext : JsonSerializerContext { }