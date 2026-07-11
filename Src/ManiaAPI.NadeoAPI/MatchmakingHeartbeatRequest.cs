using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MatchmakingHeartbeatRequest(string Code, ImmutableList<Guid> PlayWith);
