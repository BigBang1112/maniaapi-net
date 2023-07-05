using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public record UserItem : IItem
{
    public long UserId { get; set; }
    public string Name { get; set; } = default!;
    public DateTimeOffset RegisteredAt { get; set; }
    public string? CustomTitle { get; set; }
    public string? UserComments { get; set; }
    public int Tracks { get; set; }
    public int TrackPacks { get; set; }
    public int ForumPosts { get; set; }
    public int ForumThreads { get; set; }
    public int VideosCreated { get; set; }
    public int VideosPosted { get; set; }
    public int TrackCommentsReceived { get; set; }
    public int TrackCommentsGiven { get; set; }
    public int TrackAwardsReceived { get; set; }
    public int TrackAwardsGiven { get; set; }
    public bool IsSupporter { get; set; }
    public bool IsModerator { get; set; }
}