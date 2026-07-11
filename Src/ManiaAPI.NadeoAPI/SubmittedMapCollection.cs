using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record SubmittedMapCollection(ImmutableList<SubmittedMap> SubmittedMaps, int ItemCount);
