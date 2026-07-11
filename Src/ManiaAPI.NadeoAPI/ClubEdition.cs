namespace ManiaAPI.NadeoAPI;

public sealed record ClubEdition
{
    public string? Name { get; init; }
    public string? State { get; init; }
    public string? Tag { get; init; }
    public string? Description { get; init; }
    public string? IconTheme { get; init; }
    public string? DecalTheme { get; init; }
    public string? VerticalTheme { get; init; }
    public string? BackgroundTheme { get; init; }
    public string? Screen8x1Theme { get; init; }
    public string? Screen16x1Theme { get; init; }
    public string? Screen16x9Theme { get; init; }
}
