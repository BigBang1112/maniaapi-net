using ManiaAPI.TMX.Attributes;
using TmEssentials;

namespace ManiaAPI.TMX;

[Fields]
public sealed record ReplayItem : IItem
{
    public int ReplayId { get; set; }
    public TimeInt32 ReplayTime { get; set; }
    public int ReplayScore { get; set; }
    public int ReplayRespawns { get; set; }
    public DateTimeOffset ReplayAt { get; set; }
    public DateTimeOffset TrackAt { get; set; }
    public int? Position { get; set; }
    public int IsBest { get; set; }
    public int? Score { get; set; }
    public int IsLeaderboard { get; set; }
    public bool Validated { get; set; }
    public User User { get; set; } = default!;
}
