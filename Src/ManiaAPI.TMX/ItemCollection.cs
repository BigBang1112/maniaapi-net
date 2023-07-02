using System;
using System.Text.Json.Serialization;

namespace ManiaAPI.TMX;

public class ItemCollection<T> where T : IItem
{
    [JsonPropertyName("More")]
    public bool HasMorePages { get; set; }

    public T[] Results { get; set; } = Array.Empty<T>();
}
