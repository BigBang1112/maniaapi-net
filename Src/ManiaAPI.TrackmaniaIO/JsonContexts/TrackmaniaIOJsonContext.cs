using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO.JsonContexts;

[JsonSerializable(typeof(CampaignCollection))]
[JsonSerializable(typeof(Campaign))]
[JsonSerializable(typeof(Leaderboard))]
[JsonSerializable(typeof(WorldRecord[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
sealed partial class TrackmaniaIOJsonContext : JsonSerializerContext { }