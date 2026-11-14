using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ManiaAPI.NadeoAPI.Converters;

/// <summary>
/// Converts an optional date that the Nadeo API sends as a Unix timestamp in seconds, or null.
/// </summary>
/// <remarks>
/// Public so that a <see cref="JsonSerializerContext"/> outside this library can use it when it includes Nadeo API models.
/// </remarks>
public sealed class NullableDateTimeOffsetUnixConverter : JsonConverter<DateTimeOffset?>
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
