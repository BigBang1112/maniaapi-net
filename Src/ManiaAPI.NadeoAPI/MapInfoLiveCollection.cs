using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MapInfoLiveCollection(ImmutableList<MapInfoLive> MapList);