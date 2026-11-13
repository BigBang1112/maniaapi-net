using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS.Converters;

/// <summary>
/// Converts a boolean to and from a JSON string of <c>"1"</c> or <c>"0"</c>.
/// </summary>
public sealed class StringBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(bool));
        return reader.GetString() == "1";
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value ? "1" : "0");
    }
}