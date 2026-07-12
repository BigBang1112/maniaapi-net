using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public sealed partial record VideoItem : IItem
{
    public long VideoId { get; set; }
    public User Poster { get; set; } = default!;
    public User? Creator { get; set; }
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Link { get; set; } = default!;
    public string LinkId { get; set; } = default!;
    public string Thumbnail { get; set; } = default!;
    public DateTimeOffset PostedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int Length { get; set; }
    public int MapCount { get; set; }
    // public VideoMap? Map { get; set; } // in the docs, but doesn't actually exist
}