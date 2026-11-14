using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaAPI;

internal sealed record FavoriteMapCollection(ImmutableList<FavoriteMap> List);
