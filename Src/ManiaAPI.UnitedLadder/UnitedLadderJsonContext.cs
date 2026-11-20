using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.UnitedLadder;

[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(ImmutableList<PlayerRanking>))]
[JsonSerializable(typeof(ImmutableList<MapPoints>))]
[JsonSerializable(typeof(TrackLeaderboard))]
[JsonSerializable(typeof(SoloBreakdown))]
[JsonSerializable(typeof(LoginData))]
[JsonSerializable(typeof(ImmutableList<HistoricalPlayerData>))]
[JsonSerializable(typeof(ImmutableList<ProgressionDataPoint>))]
[JsonSerializable(typeof(ImmutableList<LeaderboardSnapshot>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class UnitedLadderJsonContext : JsonSerializerContext { }
