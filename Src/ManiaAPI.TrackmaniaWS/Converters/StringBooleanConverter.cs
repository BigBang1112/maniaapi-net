using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS.Converters;

internal sealed class StringBooleanConverter : JsonConverter<bool>
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