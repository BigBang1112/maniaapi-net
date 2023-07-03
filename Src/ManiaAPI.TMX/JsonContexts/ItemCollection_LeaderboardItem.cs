using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;

namespace ManiaAPI.TMX.JsonContexts;

[JsonSerializable(typeof(ItemCollection<LeaderboardItem>))]
partial class ItemCollection_LeaderboardItem : JsonSerializerContext
{
    public static JsonTypeInfo<ItemCollection<LeaderboardItem>> TypeInfo { get; } = new ItemCollection_LeaderboardItem(TmxJsonSerializerOptions.Default).ItemCollectionLeaderboardItem;
}