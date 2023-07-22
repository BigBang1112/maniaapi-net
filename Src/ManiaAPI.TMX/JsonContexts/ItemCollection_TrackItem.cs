using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;

namespace ManiaAPI.TMX.JsonContexts;

[JsonSerializable(typeof(ItemCollection<TrackItem>))]
sealed partial class ItemCollection_TrackItem : JsonSerializerContext
{
    public static JsonTypeInfo<ItemCollection<TrackItem>> TypeInfo { get; } = new ItemCollection_TrackItem(TmxJsonSerializerOptions.Create()).ItemCollectionTrackItem;
}