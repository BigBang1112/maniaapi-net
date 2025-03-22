using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ManiapubCollection(ImmutableArray<Maniapub> DisplayList);
