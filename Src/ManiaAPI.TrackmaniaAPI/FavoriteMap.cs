namespace ManiaAPI.TrackmaniaAPI;

/// <summary>
/// A map in the authenticated player's favorites.
/// </summary>
public sealed record FavoriteMap(string Uid, string Name, Guid Author, string ThumbnailUrl);
