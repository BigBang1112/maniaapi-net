using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ManiaAPI.TMX.JsonContexts;

[JsonSerializable(typeof(ItemCollection<TrackpackItem>))]
partial class ItemCollection_TrackpackItem : JsonSerializerContext
{
    public static JsonTypeInfo<ItemCollection<TrackpackItem>> TypeInfo { get; } = new ItemCollection_TrackpackItem(TmxJsonSerializerOptions.Create()).ItemCollectionTrackpackItem;
}
