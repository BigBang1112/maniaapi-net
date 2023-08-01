using System.Text.Json.Serialization;

namespace ManiaAPI.TMX.JsonContexts;

[JsonSerializable(typeof(ItemCollection<ReplayItem>))]
[JsonSerializable(typeof(ItemCollection<TrackItem>))]
[JsonSerializable(typeof(ItemCollection<TrackpackItem>))]
[JsonSerializable(typeof(ItemCollection<UserItem>))]
[JsonSerializable(typeof(ItemCollection<LeaderboardItem>))]
sealed partial class TMXJsonContext : JsonSerializerContext { }