using ManiaAPI.TrackmaniaIO.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record CompetitionItem(int Id,
                                     [property: JsonPropertyName("competitionid")] int CompetitionId,
                                     [property: JsonPropertyName("clubid")] int ClubId,
                                     string Name,
                                     [property: JsonConverter(typeof(DateTimeOffsetUnixConverter))] DateTimeOffset Timestamp,
                                     [property: JsonPropertyName("creatorplayer")] Player CreatorPlayer,
                                     [property: JsonPropertyName("latesteditorplayer")] Player LatestEditorPlayer);