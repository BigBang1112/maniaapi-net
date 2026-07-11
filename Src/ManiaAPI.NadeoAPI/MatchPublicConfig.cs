using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record MatchPublicConfig(string Script, ImmutableList<string> Maps);
