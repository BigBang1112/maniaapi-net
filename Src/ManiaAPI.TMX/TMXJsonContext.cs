using System.Text.Json.Serialization;

namespace ManiaAPI.TMX;

[JsonSerializable(typeof(ItemCollection<ReplayItem>))]
[JsonSerializable(typeof(ItemCollection<TrackItem>))]
[JsonSerializable(typeof(ItemCollection<TrackpackItem>))]
[JsonSerializable(typeof(ItemCollection<UserItem>))]
[JsonSerializable(typeof(ItemCollection<LeaderboardItem>))]
internal sealed partial class TMXJsonContext : JsonSerializerContext { }