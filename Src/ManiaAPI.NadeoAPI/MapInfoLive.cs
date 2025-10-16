using ManiaAPI.NadeoAPI.Converters;
using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.NadeoAPI;

public sealed record MapInfoLive(string Uid,
                                 Guid MapId,
                                 string Name,
                                 Guid Author,
                                 Guid Submitter,
                                 [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 AuthorTime,
                                 [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 GoldTime,
                                 [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 SilverTime,
                                 [property: JsonConverter(typeof(JsonTimeInt32Converter))] TimeInt32 BronzeTime,
                                 int NbLaps,
                                 bool Valid,
                                 string DownloadUrl,
                                 string ThumbnailUrl,
                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset UploadTimestamp,
                                 [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset UpdateTimestamp,
                                 int? FileSize,
                                 bool Public,
                                 bool Favorite,
                                 bool Playable,
                                 string MapStyle,
                                 string MapType,
                                 string CollectionName);