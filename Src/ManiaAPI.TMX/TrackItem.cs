using ManiaAPI.TMX.Attributes;
using ManiaAPI.TMX.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using TmEssentials;

namespace ManiaAPI.TMX;

[Fields]
public sealed record TrackItem : IItem
{
    public long TrackId { get; set; }
    public string TrackName { get; set; } = default!;
    public string UId { get; set; } = default!;
    [JsonConverter(typeof(TimeInt32Converter))] public TimeInt32 AuthorTime { get; set; }
    public int AuthorScore { get; set; }
    [JsonConverter(typeof(TimeInt32Converter))] public TimeInt32 GoldTarget { get; set; }
    [JsonConverter(typeof(TimeInt32Converter))] public TimeInt32 SilverTarget { get; set; }
    [JsonConverter(typeof(TimeInt32Converter))] public TimeInt32 BronzeTarget { get; set; }
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
    public ImmutableArray<Author> Authors { get; set; }
    public ImmutableArray<TrackStyle> Tags { get; set; }
    public ImmutableArray<Image>? Images { get; set; }
}
