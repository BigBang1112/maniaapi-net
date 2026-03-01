using ManiaAPI.TMX.Attributes;
using ManiaAPI.TMX.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TMX;

[Fields]
public sealed record MapItem : IItem
{
    public long MapId { get; set; }
    public string MapUid { get; set; } = default!;
    public string? OnlineMapId { get; set; }
    public string Name { get; set; } = default!;
    public string? GbxMapName { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ActivityAt { get; set; }
    public User Uploader { get; set; } = default!;
    public ImmutableList<Author>? Authors { get; set; }
    public int Type { get; set; }
    public string MapType { get; set; } = default!;
    public int Environment { get; set; }
    public int Vehicle { get; set; }
    public string? VehicleName { get; set; }
    public string? Mood { get; set; }
    public string? MoodFull { get; set; }
    public int Style { get; set; }
    public int Routes { get; set; }
    public int Difficulty { get; set; }
    public MapMedals Medals { get; set; }
    public int? CustomLength { get; set; }
    [JsonConverter(typeof(JsonBuggedInt32Converter))] public int Length { get; set; }
    public int AwardCount { get; set; }
    public int CommentCount { get; set; }
    public int DownloadCount { get; set; }
    public int ReplayCount { get; set; }
    public int ReplayType { get; set; }
    public object? ReplayWRID { get; set; }
    public int TrackValue { get; set; }
    public string TitlePack { get; set; } = default!;
    public MapFeature? Feature { get; set; }
    public bool HasThumbnail { get; set; }
    public bool HasImages { get; set; }
    public bool IsPublic { get; set; }
    public bool IsListed { get; set; }
    public bool ServerSizeExceeded { get; set; }
    public string? AuthorComments { get; set; }
    public ImmutableList<Tag>? Tags { get; set; }
    public ImmutableList<MapImage>? Images { get; set; }
    public MappackInfo? Mappack { get; set; }
    // public MapOnlineWR? OnlineWR { get; set; } // use Nadeo API!
    // public object? UserOnlineRecord { get; set; } // login required
    // public object? UserRecord { get; set; } // login required
    // public object? ReplayWR { get; set; }  // null in example, 400 on query. Probably login required?
    // public bool? InBookmarks { get; set; } // login required
    // public int? TimeStampAt { get; set; } // Requires videoid parameter
}