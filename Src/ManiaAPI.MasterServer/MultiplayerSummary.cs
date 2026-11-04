using System.Collections.Immutable;

namespace ManiaAPI.MasterServer;

public sealed record MultiplayerSummary(
    string Zone, 
    int Count,
    DateTimeOffset Timestamp,
    ImmutableArray<LadderRank> LadderPoints);
