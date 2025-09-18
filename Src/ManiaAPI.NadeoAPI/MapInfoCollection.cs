using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MapInfoCollection(ImmutableList<MapInfo> MapList);