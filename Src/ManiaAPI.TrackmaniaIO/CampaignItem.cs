using ManiaAPI.Base.Converters;
using System.Text.Json.Serialization;

namespace ManiaAPI.TrackmaniaIO;

public abstract record CampaignItem : ICampaignItem
{
    public int Id { get; init; }
    public string Name { get; init; }

    [JsonConverter(typeof(DateTimeOffsetUnixConverter))]
    public DateTimeOffset Timestamp { get; init; }

    public int MapCount { get; init; }

    public CampaignItem(int id, string name, DateTimeOffset timestamp, int mapCount)
    {
        Id = id;
        Name = name;
        Timestamp = timestamp;
        MapCount = mapCount;
    }

    public abstract Task<ICampaign> GetDetailsAsync(CancellationToken cancellationToken = default);

    public override string ToString()
    {
        return Name;
    }
}
