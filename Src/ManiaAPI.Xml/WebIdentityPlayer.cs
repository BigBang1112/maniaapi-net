using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public sealed record WebIdentityPlayer(string Login, ImmutableList<WebIdentity> WebIdentities);