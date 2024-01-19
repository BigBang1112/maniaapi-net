using System.Text.Json.Serialization;

namespace ManiaAPI.TMX;

public sealed class ItemCollection<T> where T : IItem
{
    [JsonPropertyName("More")]
    public bool HasMoreItems { get; set; }

    public T[] Results { get; set; } = [];
}
