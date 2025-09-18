using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public sealed record WaitingParams(ImmutableList<MasterServerInfo> MasterServers);