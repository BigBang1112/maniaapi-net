using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.ManiaPlanetAPI.Converters;

/// <summary>
/// Converts a <see cref="DateTimeOffset"/> to and from Unix time in seconds.
/// The seconds can be fractional, like the <c>iat</c>, <c>nbf</c> and <c>exp</c> claims of an access token, and are written back the same way.
/// </summary>
public sealed class DateTimeOffsetUnixConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTimeOffset));

        if (reader.TryGetInt64(out var seconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        // decimal keeps every digit of the microseconds, which double would round
        var ticks = decimal.Round(reader.GetDecimal() * TimeSpan.TicksPerSecond);
        return DateTimeOffset.UnixEpoch.AddTicks((long)ticks);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        var ticks = value.UtcTicks - DateTimeOffset.UnixEpoch.UtcTicks;

        if (ticks % TimeSpan.TicksPerSecond == 0)
        {
            writer.WriteNumberValue(ticks / TimeSpan.TicksPerSecond);
        }
        else
        {
            writer.WriteNumberValue((decimal)ticks / TimeSpan.TicksPerSecond);
        }
    }
}
