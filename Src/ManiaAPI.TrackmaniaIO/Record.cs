using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Record(int Position,
                            [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 Time,
                            int Score,
                            [property: JsonPropertyName("filename")] string? FileName,
                            DateTimeOffset? Timestamp,
                            string? Url,
                            Player Player)
{
    public override string ToString()
    {
        return $"{Time} by {Player}";
    }
}