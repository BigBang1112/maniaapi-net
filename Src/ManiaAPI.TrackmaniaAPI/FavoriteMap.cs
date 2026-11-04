using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaAPI;

/// <summary>
/// A map in the authenticated player's favorites.
/// </summary>
public sealed record FavoriteMap(string Uid, string Name, Guid Author, string ThumbnailUrl);

internal sealed record FavoriteMapCollection(ImmutableList<FavoriteMap> List);

internal sealed record FavoriteMapRequest(string Uid);
