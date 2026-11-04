using System.Collections.Immutable;

namespace ManiaAPI.MasterServer;

public sealed record WaitingParams(int WaitingQueueDuration, string? WaitingQueueMessage, ImmutableList<MasterServerInfo> MasterServers);