using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public sealed record WaitingParams(int WaitingQueueDuration, string? WaitingQueueMessage, ImmutableList<MasterServerInfo> MasterServers);