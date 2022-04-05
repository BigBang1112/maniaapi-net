using ManiaAPI.Base.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public record Campaign : ICampaign
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Media { get; init; } = "";

    [JsonConverter(typeof(NullableDateTimeOffsetUnixConverter))]
    public DateTimeOffset? CreationTime { get; init; }

    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset PublishTime { get; init; }

    public int ClubId { get; init; }
    public string ClubDecal { get; init; } = "";
    public string ClubIcon { get; init; } = "";
    public string LeaderboardUid { get; init; } = "";
    public Map[] Playlist { get; init; } = Array.Empty<Map>();

    public async Task<Leaderboard> GetMapLeaderboardAsync(Map map, CancellationToken cancellationToken = default)
    {
        return await TrackmaniaIO.GetLeaderboardAsync(LeaderboardUid, map.MapUid, cancellationToken);
    }

    public async Task<WorldRecord[]> GetRecentWorldRecordsAsync(CancellationToken cancellationToken = default)
    {
        return await TrackmaniaIO.GetRecentWorldRecordsAsync(LeaderboardUid, cancellationToken);
    }

    public override string ToString()
    {
        return Name;
    }
}
