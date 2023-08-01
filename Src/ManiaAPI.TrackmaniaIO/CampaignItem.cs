using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record CampaignItem(int Id,
                                  string Name,
                                  [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset Timestamp,
                                  [property: JsonPropertyName("mapcount")] int MapCount,
                                  bool Tracked)
{
    public override string ToString() => Name;
}
