namespace ManiaAPI.TMX;

/// <summary>
/// A track tag on a TMX site with a description of when to use it.
/// </summary>
public record struct TagDescription(int Id, string Name, string Description);
