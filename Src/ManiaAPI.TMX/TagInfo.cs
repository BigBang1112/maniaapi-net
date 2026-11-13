namespace ManiaAPI.TMX;

/// <summary>
/// A tag of maps, mappacks or tracks.
/// </summary>
/// <param name="ID">Tag ID.</param>
/// <param name="Name">Tag name.</param>
/// <param name="Color">Tag color on MX sites, null on TMX sites.</param>
public record struct TagInfo(int ID, string Name, string? Color);
