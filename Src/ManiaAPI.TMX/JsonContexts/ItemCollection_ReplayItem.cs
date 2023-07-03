using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.TMX.JsonContexts;

[JsonSerializable(typeof(ItemCollection<ReplayItem>))]
partial class ItemCollection_ReplayItem : JsonSerializerContext
{
    public static JsonTypeInfo<ItemCollection<ReplayItem>> TypeInfo { get; } = new ItemCollection_ReplayItem(TmxJsonSerializerOptions.Default).ItemCollectionReplayItem;
}
