using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.TMX;

public record TrackSearchItem : IItem
{
    private static string? fieldsQuery;

    public int TrackId { get; init; }
    public string TrackName { get; init; } = default!;
    public TrackType PrimaryType { get; init; }
    public Environment Environment { get; init; }
    public Car Car { get; init; }
    public Mood Mood { get; init; }
    public TrackStyle Style { get; init; }
    public TrackRoutes Routes { get; init; }
    public int Length { get; init; }
    public int Difficulty { get; init; }
    public int Awards { get; init; }

    [JsonPropertyName("CommentCt")]
    public int CommentCount { get; init; }

    public DateTime UploadedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime ActivityAt { get; init; }
    public int ReplayType { get; init; }
    public int ReplayScore { get; init; }
    public bool HasThumbnail { get; init; }
    public bool IsPublic { get; init; }

    [JsonPropertyName("UId")]
    public string MapUid { get; init; } = default!;
    public TimeInt32 AuthorTime { get; init; }
    public int AuthorScore { get; init; }

    [JsonPropertyName("GoldTarget")]
    public TimeInt32 GoldTime { get; init; }

    [JsonPropertyName("SilverTarget")]
    public TimeInt32 SilverTime { get; init; }

    [JsonPropertyName("BronzeTarget")]
    public TimeInt32 BronzeTime { get; init; }

    public TrackAuthor[] Authors { get; init; } = default!;
    public Image[] Images { get; init; } = default!;
    public TrackStyle[] Tags { get; init; } = default!;
    public User Uploader { get; init; } = default!;
    public WRReplay? WRReplay { get; init; }
    public UserReplay? UserReplay { get; init; }

    public async Task<ItemCollection<ReplayItem>> GetReplaysAsync(TMX tmx, CancellationToken cancellationToken = default)
    {
        return await tmx.GetReplaysAsync(TrackId, cancellationToken);
    }
}
