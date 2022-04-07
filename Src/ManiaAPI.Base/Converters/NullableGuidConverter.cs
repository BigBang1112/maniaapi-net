using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.Base.Converters;

public class NullableGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(Guid?));

        if (reader.TryGetGuid(out Guid value))
        {
            return value;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteStringValue("");
        }
    }
}
