using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS.Converters;

/// <summary>
/// Converts a 32-bit integer to and from a JSON string. A JSON number is read as well, as some routes send one.
/// </summary>
public sealed class StringInt32Converter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(int));

        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32();
        }

        _ = int.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value);
        return value;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
