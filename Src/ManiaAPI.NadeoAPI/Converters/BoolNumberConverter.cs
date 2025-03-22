using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.Converters;

internal sealed class BoolNumberConverter : JsonConverter<bool>
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