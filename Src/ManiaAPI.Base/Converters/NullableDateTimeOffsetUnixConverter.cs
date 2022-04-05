using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.Base.Converters;

public class NullableDateTimeOffsetUnixConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTimeOffset?));

        var unix = reader.GetInt64();

        if (unix == 0)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(unix);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value?.ToUnixTimeSeconds() ?? 0);
    }
}
