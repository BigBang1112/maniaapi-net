using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.Converters;

/// <summary>
/// Converts a date that the Nadeo API sends as a Unix timestamp in seconds.
/// </summary>
/// <remarks>
/// Public so that a <see cref="JsonSerializerContext"/> outside this library can use it when it includes Nadeo API models.
/// </remarks>
public sealed class DateTimeOffsetUnixConverter : JsonConverter<DateTimeOffset>
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
