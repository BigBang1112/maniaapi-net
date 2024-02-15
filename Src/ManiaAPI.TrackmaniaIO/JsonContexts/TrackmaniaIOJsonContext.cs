using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO.JsonContexts;

[JsonSerializable(typeof(CampaignCollection))]
[JsonSerializable(typeof(Campaign))]
[JsonSerializable(typeof(Leaderboard))]
[JsonSerializable(typeof(ImmutableArray<WorldRecord>))]
[JsonSerializable(typeof(AdCollection))]
[JsonSerializable(typeof(TrackOfTheDayMonth))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class TrackmaniaIOJsonContext : JsonSerializerContext { }