using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

internal sealed record PlayerListRequest(ImmutableList<PlayerIdRequest> ListPlayer);
