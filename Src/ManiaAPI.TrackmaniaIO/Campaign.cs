using ManiaAPI.TrackmaniaIO.Converters;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public sealed record Campaign(int Id,
                              string Name,
                              string Media,
                              [property: JsonConverter(typeof(NullableDateTimeOffsetUnixConverter)), JsonPropertyName("creationtime")] DateTimeOffset? CreationTime,
                              [property: JsonConverter(typeof(DateTimeOffsetUnixConverter)), JsonPropertyName("publishtime")] DateTimeOffset PublishTime,
                              [property: JsonPropertyName("clubid")] int ClubId,
                              [property: JsonPropertyName("clubname")] string? ClubName,
                              [property: JsonPropertyName("leaderboarduid")] string LeaderboardUid,
                              ImmutableArray<Map> Playlist,
                              Media Mediae,
                              bool Tracked)
{
    public override string ToString() => Name;
}
