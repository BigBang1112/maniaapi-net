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

    /// <summary>
    /// When the video info was last updated. Only available on TMX sites, null on MX sites.
    /// </summary>
    [TmxOnly] public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Length of the video in seconds.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Number of maps featured in the video. Only available on MX sites, 0 on TMX sites.
    /// </summary>
    [MxOnly] public int MapCount { get; set; }
    // public VideoMap? Map { get; set; } // in the docs, but doesn't actually exist
}
