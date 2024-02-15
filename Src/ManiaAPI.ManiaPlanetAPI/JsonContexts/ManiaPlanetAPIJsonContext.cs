using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI.JsonContexts;

[JsonSerializable(typeof(JwtPayloadManiaPlanetAPI))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ImmutableArray<Title>))]
[JsonSerializable(typeof(ImmutableArray<DedicatedAccount>))]
[JsonSerializable(typeof(ImmutableArray<Map>))]
[JsonSerializable(typeof(ImmutableArray<Zone>))]
[JsonSerializable(typeof(ImmutableArray<TitleScript>))]
[JsonSerializable(typeof(ImmutableArray<OnlineServer>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ManiaPlanetAPIJsonContext : JsonSerializerContext { }