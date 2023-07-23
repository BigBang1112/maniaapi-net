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
[JsonSerializable(typeof(TrackOfTheDayCollection))]
[JsonSerializable(typeof(TrackOfTheDayInfo))]
[JsonSerializable(typeof(CampaignCollection))]
[JsonSerializable(typeof(SeasonPlayerRankingCollection))]
[JsonSerializable(typeof(PlayerZone[]))]
[JsonSerializable(typeof(Dictionary<string, ApiRoute>))]
[JsonSerializable(typeof(Zone[]))]
[JsonSerializable(typeof(PlayerClubTag[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
sealed partial class NadeoAPIJsonContext : JsonSerializerContext { }
