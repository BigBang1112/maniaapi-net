using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.Converters;

/// <summary>
/// Converts a boolean that the Nadeo API sends as the number 1 or 0.
/// </summary>
/// <remarks>
/// Public so that a <see cref="JsonSerializerContext"/> outside this library can use it when it includes Nadeo API models.
/// </remarks>
public sealed class BoolNumberConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var number = reader.GetInt32();

        return number == 1;
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value ? 1 : 0);
    }
}