using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ManiaAPI.TrackmaniaIO.Converters;

internal sealed class NullableDateTimeOffsetUnixConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTimeOffset?));
        return DateTimeOffset.FromUnixTimeMilliseconds((long)(reader.GetDouble() * 1000));
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value.ToUnixTimeSeconds());
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
