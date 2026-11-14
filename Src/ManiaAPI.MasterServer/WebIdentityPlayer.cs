using System.Collections.Immutable;

namespace ManiaAPI.MasterServer;

public sealed record WebIdentityPlayer(string Login, ImmutableList<WebIdentity> WebIdentities);