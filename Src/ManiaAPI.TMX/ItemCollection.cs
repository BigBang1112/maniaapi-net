using System.Text.Json.Serialization;

namespace ManiaAPI.TMX;

public class ItemCollection<T> where T : IItem
{
    [JsonPropertyName("More")]
    public bool HasMorePages { get; init; }

    public T[] Results { get; init; } = default!;
}
