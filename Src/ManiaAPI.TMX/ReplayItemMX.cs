using ManiaAPI.TMX.Attributes;
using System.Text.Json.Serialization;
using TmEssentials;
using TmEssentials.Converters;

namespace ManiaAPI.TMX;

[Fields]
public sealed partial record ReplayItemMX : IItem
{
    public long ReplayId { get; set; }
    public User User { get; set; } = default!;
    public ReplayMap Map { get; set; } = default!;
    [JsonConverter(typeof(JsonTimeInt32Converter))] public TimeInt32 ReplayTime { get; set; }
    public int ReplayPoints { get; set; }
    public int Respawns { get; set; }
    public DateTimeOffset ReplayAt { get; set; }
    public DateTimeOffset TrackAt { get; set; }
    public int? Position { get; set; }
    public int Score { get; set; }
    public object? Season { get; set; } // int in docs, object in actual response
    public bool HasFile { get; set; }
}