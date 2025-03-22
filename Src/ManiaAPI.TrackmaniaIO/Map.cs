using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Map(Guid Author,
                         string Name,
                         string MapType,
                         string MapStyle,
                         [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 AuthorScore,
                         [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 GoldScore,
                         [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 SilverScore,
                         [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 BronzeScore,
                         string CollectionName,
                         [property: JsonPropertyName("filename")] string FileName,
                         bool IsPlayable,
                         Guid MapId,
                         string MapUid,
                         Guid Submitter,
                         DateTimeOffset Timestamp,
                         string FileUrl,
                         string ThumbnailUrl,
                         [property: JsonPropertyName("authorplayer")] Author AuthorPlayer,
                         [property: JsonPropertyName("submitterplayer")] Author SubmitterPlayer,
                         int ExchangeId)
{
    public override string ToString()
    {
        return $"{Name} by {AuthorPlayer.Name}";
    }
}
