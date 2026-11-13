using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS.Converters;

/// <summary>
/// Converts a 32-bit integer to and from a JSON string.
/// </summary>
public sealed class StringInt32Converter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(int));
        _ = int.TryParse(reader.GetString(), out int value);
        return value;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}