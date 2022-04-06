using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.Base.Converters;

public class NullableIntConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(int?));
        
        var number = reader.GetInt64();

        if (number == uint.MaxValue)
        {
            return null;
        }

        return (int)number;
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue((uint)value);
        }
        else
        {
            writer.WriteNumberValue(uint.MaxValue);
        }
    }
}
