using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.NadeoAPI;

public sealed record MapInfo(Guid Author,
                             [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 AuthorScore,
                             [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 BronzeScore,
                             string CollectionName,
                             [property: JsonPropertyName("filename")] string FileName,
                             [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 GoldScore,
                             bool IsPlayable,
                             Guid MapId,
                             string MapStyle,
                             string MapType,
                             string MapUid,
                             string Name,
                             [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 SilverScore,
                             Guid Submitter,
                             DateTimeOffset Timestamp,
                             string FileUrl,
                             string ThumbnailUrl);