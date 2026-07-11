using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MapFavoriteCollection(ImmutableList<MapFavorite> MapFavoriteList, int Count);
