using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record AdCollection(ImmutableList<Ad> Ads);