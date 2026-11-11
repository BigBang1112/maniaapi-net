using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.NadeoAPI.Converters;

/// <summary>
/// Converts an optional GUID that the Nadeo API sends as an empty string when there is none.
/// </summary>
/// <remarks>
/// Public so that a <see cref="JsonSerializerContext"/> outside this library can use it when it includes Nadeo API models.
/// </remarks>
public sealed class NullableGuidConverter : JsonConverter<Guid?>
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
