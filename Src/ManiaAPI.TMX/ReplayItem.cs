using ManiaAPI.TMX.Attributes;
using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.TMX;

[Fields]
public sealed record ReplayItem : IItem
{
    public int ReplayId { get; set; }
    [JsonConverter(typeof(JsonTimeInt32Converter))] public TimeInt32 ReplayTime { get; set; }
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
