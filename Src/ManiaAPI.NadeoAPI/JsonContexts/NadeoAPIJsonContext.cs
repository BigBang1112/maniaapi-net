using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.JsonContexts;

[JsonSerializable(typeof(Account[]))]
[JsonSerializable(typeof(AuthorizationBody))]
[JsonSerializable(typeof(AuthorizationResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(JwtPayloadNadeoAPI))]
[JsonSerializable(typeof(MapInfoCollection))]
[JsonSerializable(typeof(MapRecord[]))]
[JsonSerializable(typeof(MedalRecordCollection))]
[JsonSerializable(typeof(ManiapubCollection[]))]
[JsonSerializable(typeof(TopLeaderboardCollection))]
[JsonSerializable(typeof(UbisoftAuthenticationTicket))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
sealed partial class NadeoAPIJsonContext : JsonSerializerContext
{
}
