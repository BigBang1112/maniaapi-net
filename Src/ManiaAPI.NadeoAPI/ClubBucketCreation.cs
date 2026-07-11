namespace ManiaAPI.NadeoAPI;

public sealed record ClubBucketCreation
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public int? FolderId { get; init; }
}
