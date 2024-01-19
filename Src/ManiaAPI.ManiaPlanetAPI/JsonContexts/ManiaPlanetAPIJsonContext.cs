using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI.JsonContexts;

[JsonSerializable(typeof(JwtPayloadManiaPlanetAPI))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(Title[]))]
[JsonSerializable(typeof(DedicatedAccount[]))]
[JsonSerializable(typeof(Map[]))]
[JsonSerializable(typeof(Zone[]))]
[JsonSerializable(typeof(TitleScript[]))]
[JsonSerializable(typeof(OnlineServer[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ManiaPlanetAPIJsonContext : JsonSerializerContext { }