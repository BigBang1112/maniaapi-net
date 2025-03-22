using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record TopLeaderboard(Guid ZoneId, string ZoneName, ImmutableArray<Record> Top);