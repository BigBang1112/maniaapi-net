using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Ad(Guid Uid,
                        string Name,
                        string Type,
                        string Url,
                        string Img2x3,
                        string Img16x9,
                        string Img64x10,
                        string Img64x41,
                        string Media,
                        [property: JsonPropertyName("displayformat")] string DisplayFormat,
                        [property: JsonConverter(typeof(DateTimeOffsetUnixConverter)), JsonPropertyName("endtime")] DateTimeOffset EndTime);