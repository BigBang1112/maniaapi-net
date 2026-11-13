using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TMX;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)] // TMX sites return "Id" where MX sites return "ID"
[JsonSerializable(typeof(ItemCollection<ReplayItem>))]
[JsonSerializable(typeof(ItemCollection<TrackItem>))]
[JsonSerializable(typeof(ItemCollection<TrackpackItem>))]
[JsonSerializable(typeof(ItemCollection<UserItem>))]
[JsonSerializable(typeof(ItemCollection<LeaderboardItem>))]
[JsonSerializable(typeof(TagDescription[]))]
[JsonSerializable(typeof(ItemCollection<MapItem>))]
[JsonSerializable(typeof(ItemCollection<ReplayItemMX>))]
[JsonSerializable(typeof(ItemCollection<UserItemMX>))]
[JsonSerializable(typeof(ItemCollection<VideoItem>))]
[JsonSerializable(typeof(ItemCollection<MappackItem>))]
[JsonSerializable(typeof(SearchOrderItem[]))]
[JsonSerializable(typeof(TagInfo[]))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(ImmutableList<IXItem>))]
[JsonSerializable(typeof(ImmutableList<IXItemSet>))]
[JsonSerializable(typeof(IXBasket))]
[JsonSerializable(typeof(IXSearchResult<IXItem>))]
[JsonSerializable(typeof(IXSearchResult<IXItemSet>))]
internal sealed partial class TMXJsonContext : JsonSerializerContext { }