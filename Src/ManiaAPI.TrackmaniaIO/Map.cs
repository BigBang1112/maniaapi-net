using TmEssentials;

namespace ManiaAPI.TrackmaniaIO;

public record Map
{
    public Guid Author { get; init; }
    public string Name { get; init; } = "";
    public string MapType { get; init; } = "";
    public string MapStyle { get; init; } = "";
    public TimeInt32 AuthorScore { get; init; }
    public TimeInt32 GoldScore { get; init; }
    public TimeInt32 SilverScore { get; init; }
    public TimeInt32 BronzeScore { get; init; }
    public string CollectionName { get; init; } = "";
    public string FileName { get; init; } = "";
    public bool IsPlayable { get; init; }
    public Guid MapId { get; init; }
    public string MapUid { get; init; } = "";
    public Guid Submitter { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string FileUrl { get; init; } = "";
    public string ThumbnailUrl { get; init; } = "";
    public Author AuthorPlayer { get; init; }
    public Author SubmitterPlayer { get; init; }
    public int ExchangeId { get; init; }

    public Map(Author authorPlayer, Author submitterPlayer)
    {
        AuthorPlayer = authorPlayer;
        SubmitterPlayer = submitterPlayer;
    }

    public override string ToString()
    {
        return $"{Name} by {AuthorPlayer.Name}";
    }
}
