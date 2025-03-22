using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Record(int Position,
                            [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 Time,
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