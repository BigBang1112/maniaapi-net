using ManiaAPI.TMX.Attributes;
using TmEssentials;

namespace ManiaAPI.TMX;

[Fields]
public record TrackItem : IItem
{
    public long TrackId { get; set; }
    public string TrackName { get; set; } = default!;
    public string UId { get; set; } = default!;
    public TimeInt32 AuthorTime { get; set; }
    public int AuthorScore { get; set; }
    public TimeInt32 GoldTarget { get; set; }
    public TimeInt32 SilverTarget { get; set; }
    public TimeInt32 BronzeTarget { get; set; }
    public User Uploader { get; set; } = default!;
    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset ActivityAt { get; set; }
    public TrackType PrimaryType { get; set; }
    public int TrackValue { get; set; }
    public string? AuthorComments { get; set; }
    public TrackStyle Style { get; set; }
    public TrackRoutes Routes { get; set; }
    public int Difficulty { get; set; }
    public Environment Environment { get; set; }
    public Car Car { get; set; }
    public Mood Mood { get; set; }
    public int Awards { get; set; }
    public int Comments { get; set; }
    public int ReplayType { get; set; }
    public bool HasThumbnail { get; set; }
    public WRReplay? WRReplay { get; set; }
    public UserReplay? UserReplay { get; set; }
    public Author[] Authors { get; set; } = default!;
    public TrackStyle[] Tags { get; set; } = default!;
    public Image[]? Images { get; set; }
}
