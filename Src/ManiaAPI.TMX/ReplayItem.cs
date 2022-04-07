using TmEssentials;

namespace ManiaAPI.TMX;

public record ReplayItem : IItem
{
    public int ReplayId { get; init; }
    public TimeInt32 ReplayTime { get; init; }
    public int ReplayScore { get; init; }
    public int ReplayRespawns { get; init; }
    public DateTimeOffset ReplayAt { get; init; }
    public DateTimeOffset TrackAt { get; init; }
    public int? Position { get; init; }
    public int IsBest { get; init; }
    public int? Score { get; init; }
    public int IsLeaderboard { get; init; }
    public bool Validated { get; init; }
    public User User { get; init; } = default!;
}
