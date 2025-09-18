using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI;

[JsonSerializable(typeof(JwtPayloadManiaPlanetAPI))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ImmutableList<Title>))]
[JsonSerializable(typeof(ImmutableList<DedicatedAccount>))]
[JsonSerializable(typeof(ImmutableList<Map>))]
[JsonSerializable(typeof(ImmutableList<Zone>))]
[JsonSerializable(typeof(ImmutableList<TitleScript>))]
[JsonSerializable(typeof(ImmutableList<OnlineServer>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ManiaPlanetAPIJsonContext : JsonSerializerContext { }