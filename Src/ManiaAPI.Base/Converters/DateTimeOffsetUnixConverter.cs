using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.Base.Converters;

public class DateTimeOffsetUnixConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTimeOffset));
        return DateTimeOffset.FromUnixTimeMilliseconds((long)(reader.GetDouble() * 1000));
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
