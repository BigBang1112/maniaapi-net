namespace ManiaAPI.NadeoAPI;

public sealed record ClubNewsCreation
{
    public required string Name { get; init; }
    public string? Headline { get; init; }
    public string? Body { get; init; }
    public int? FolderId { get; init; }
}
