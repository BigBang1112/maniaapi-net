using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Club(int Id,
                          string Name,
                          string Tag,
                          string Description,
                          string IconUrl,
                          string LogoUrl,
                          string DecalUrl,
                          string Screen16x9Url,
                          string Screen64x41Url,
                          string DecalSponsor4x1Url,
                          string Screen8x1Url,
                          string Screen16x1Url,
                          string VerticalUrl,
                          string BackgroundUrl,
                          [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset CreationTimestamp,
                          int PopularityLevel,
                          string State,
                          bool Featured,
                          bool Verified,
                          [property: JsonPropertyName("membercount")] int MemberCount,
                          [property: JsonPropertyName("creatorplayer")] Player CreatorPlayer,
                          [property: JsonPropertyName("latesteditorplayer")] Player LatestEditorPlayer);