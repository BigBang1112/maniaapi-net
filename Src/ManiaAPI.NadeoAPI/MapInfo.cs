using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.NadeoAPI;

public sealed record MapInfo(Guid Author,
                             [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 AuthorScore,
                             [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 BronzeScore,
                             string CollectionName,
                             [property: JsonPropertyName("filename")] string FileName,
                             [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 GoldScore,
                             bool IsPlayable,
                             Guid MapId,
                             string MapStyle,
                             string MapType,
                             string MapUid,
                             string Name,
                             [property: JsonConverter(typeof(TimeInt32Converter))] TimeInt32 SilverScore,
                             Guid Submitter,
                             DateTimeOffset Timestamp,
                             string FileUrl,
                             string ThumbnailUrl);