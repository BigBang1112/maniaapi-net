using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaWS.Converters;

/// <summary>
/// Converts a list of 32-bit integers to and from a comma-separated JSON string like <c>"1,330"</c>.
/// </summary>
public sealed class StringInt32ListConverter : JsonConverter<ImmutableList<int>>
{
    public override ImmutableList<int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.Parse(x, NumberStyles.Integer, CultureInfo.InvariantCulture))
            .ToImmutableList();
    }

    public override void Write(Utf8JsonWriter writer, ImmutableList<int> value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(string.Join(',', value.Select(x => x.ToString(CultureInfo.InvariantCulture))));
    }
}
