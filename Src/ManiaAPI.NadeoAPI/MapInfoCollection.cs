using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MapInfoCollection(ImmutableArray<MapInfo> MapList);