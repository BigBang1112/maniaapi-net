using ManiaAPI.TMX.Attributes;

namespace ManiaAPI.TMX;

public record TrackSearchFilters : RequestParameters
{
    [Query("count", Default = 40)]
    public int? Count { get; init; }

    [Query("author")]
    public string? Author { get; init; }

    [Query("environment")]
    public Environment? Environment { get; init; }

    [Query("vehicle")]
    public Environment? Car { get; init; }

    [Query("primarytype")]
    public TrackType? PrimaryType { get; init; }

    [Query("tag")]
    public TrackStyle? Tag { get; init; }

    [Query("mood")]
    public Mood? Mood { get; init; }

    [Query("difficulty")]
    public Difficulty? Difficulty { get; init; }

    [Query("routes")]
    public TrackRoutes? Routes { get; init; }

    [Query("lbtype")]
    public LeaderboardType? LeaderboardType { get; init; }

    [Query("order1")]
    public TrackOrder? PrimaryOrder { get; init; }

    [Query("order2")]
    public TrackOrder? SecondaryOrder { get; init; }

    [Query("after")]
    public int? AfterTrackId { get; init; }

    [Query("before")]
    public int? BeforeTrackId { get; init; }
}
