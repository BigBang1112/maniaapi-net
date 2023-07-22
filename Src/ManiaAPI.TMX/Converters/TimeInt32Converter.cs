using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using TmEssentials;

namespace ManiaAPI.TMX.Converters;

sealed class TimeInt32Converter : JsonConverter<TimeInt32>
{
    public override TimeInt32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(TimeInt32));
        return new TimeInt32(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, TimeInt32 value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.TotalMilliseconds);
    }
}
