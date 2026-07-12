using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

[Fields]
public sealed partial record UserItemMX : IItem
{
    public string Name { get; set; } = default!;
    public long UserId { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public string? CustomTitle { get; set; }
    public bool IsSupporter { get; set; }
    public bool IsModerator { get; set; }
    public string? Bio { get; set; }
    public string? ModBio { get; set; }
    public int MapCount { get; set; }
    public int MappackCount { get; set; }
    public int ThreadCount { get; set; }
    public int PostCount { get; set; }
    public int AwardsReceivedCount { get; set; }
    public int AwardsGivenCount { get; set; }
    public int CommentsReceivedCount { get; set; }
    public int CommentsGivenCount { get; set; }
    public int VideosCreatedCount { get; set; }
    public int VideosPostedCount { get; set; }
    public int FavoritesReceivedCount { get; set; }
    public int ReplayCount { get; set; }
}