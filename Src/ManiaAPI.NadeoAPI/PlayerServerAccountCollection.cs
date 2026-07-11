using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record PlayerServerAccountCollection(ImmutableList<PlayerServerAccount> PlayerServerAccount, int ItemCount);
