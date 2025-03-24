using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc;

public sealed record WaitingParams(ImmutableArray<MasterServerInfo> MasterServers);