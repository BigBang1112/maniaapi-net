using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS;

[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(ImmutableList<PlayerTag>))]
[JsonSerializable(typeof(PlayerRanking))]
[JsonSerializable(typeof(Zone))]
[JsonSerializable(typeof(ImmutableList<Zone>))]
[JsonSerializable(typeof(Manialink))]
[JsonSerializable(typeof(PlayerLeaderboard))]
[JsonSerializable(typeof(ChallengeLeaderboard))]
[JsonSerializable(typeof(ZoneLeaderboard))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(ManiaHomeNotification))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class TrackmaniaWSJsonContext : JsonSerializerContext { }
