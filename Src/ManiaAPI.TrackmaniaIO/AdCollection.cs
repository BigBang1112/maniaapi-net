using System.Collections.Immutable;

namespace ManiaAPI.TrackmaniaIO;

public sealed record AdCollection(ImmutableArray<Ad> Ads);