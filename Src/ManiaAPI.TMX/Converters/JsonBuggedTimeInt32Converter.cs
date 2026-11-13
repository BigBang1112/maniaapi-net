using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.TMX.Converters;

/// <summary>
/// Reads a <see cref="TmEssentials.TimeInt32"/> from Exchange values that may exceed the 32-bit range, and writes it as milliseconds.
/// </summary>
public sealed class JsonBuggedTimeInt32Converter : JsonConverter<TimeInt32>
{
    public override TimeInt32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(TimeInt32));
        return new TimeInt32((int)reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, TimeInt32 value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.TotalMilliseconds);
    }
}