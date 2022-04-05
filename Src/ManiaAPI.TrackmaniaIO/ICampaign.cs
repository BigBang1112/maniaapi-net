namespace ManiaAPI.TrackmaniaIO;

public interface ICampaign
{
    string ClubDecal { get; init; }
    string ClubIcon { get; init; }
    int ClubId { get; init; }
    DateTimeOffset? CreationTime { get; init; }
    int Id { get; init; }
    string LeaderboardUid { get; init; }
    string Media { get; init; }
    string Name { get; init; }
    Map[] Playlist { get; init; }
    DateTimeOffset PublishTime { get; init; }

    Task<Leaderboard> GetMapLeaderboardAsync(Map map, CancellationToken cancellationToken = default);
    Task<WorldRecord[]> GetRecentWorldRecordsAsync(CancellationToken cancellationToken = default);
}
