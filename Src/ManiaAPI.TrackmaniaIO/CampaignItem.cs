using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record CampaignItem(int Id,
                                  string Name,
                                  [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset Timestamp,
                                  [property: JsonPropertyName("mapcount")] int MapCount,
                                  bool Tracked,
                                  bool Official,
                                  bool Seasonal,
                                  bool Weekly,
                                  [property: JsonPropertyName("clubid")] int? ClubId,
                                  [property: JsonPropertyName("clubname")] string? ClubName,
                                  [property: JsonPropertyName("mediaurl")] string? MediaUrl,
                                  [property: JsonPropertyName("creatorplayer")] Player? CreatorPlayer,
                                  [property: JsonPropertyName("latesteditorplayer")] Player? LatestEditorPlayer)
{
    public override string ToString() => Name;
}
