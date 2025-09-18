using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

[JsonSerializable(typeof(CampaignCollection))]
[JsonSerializable(typeof(Campaign))]
[JsonSerializable(typeof(Leaderboard))]
[JsonSerializable(typeof(ImmutableList<WorldRecord>))]
[JsonSerializable(typeof(ClubCollection))]
[JsonSerializable(typeof(ClubMemberCollection))]
[JsonSerializable(typeof(ClubActivityCollection))]
[JsonSerializable(typeof(AdCollection))]
[JsonSerializable(typeof(TrackOfTheDayMonth))]
[JsonSerializable(typeof(ClubRoomCollection))]
[JsonSerializable(typeof(ClubRoom))]
[JsonSerializable(typeof(CompetitionCollection))]
[JsonSerializable(typeof(Competition))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class TrackmaniaIOJsonContext : JsonSerializerContext { }