using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;

namespace ManiaAPI.TMX.JsonContexts;

[JsonSerializable(typeof(ItemCollection<UserItem>))]
partial class ItemCollection_UserItem : JsonSerializerContext
{
    public static JsonTypeInfo<ItemCollection<UserItem>> TypeInfo { get; } = new ItemCollection_UserItem(TmxJsonSerializerOptions.Create()).ItemCollectionUserItem;
}