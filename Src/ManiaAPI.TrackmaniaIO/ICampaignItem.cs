namespace ManiaAPI.TrackmaniaIO;

public interface ICampaignItem
{
    int Id { get; init; }
    int MapCount { get; init; }
    string Name { get; init; }
    DateTimeOffset Timestamp { get; init; }

    Task<ICampaign> GetDetailsAsync(CancellationToken cancellationToken = default);
}
