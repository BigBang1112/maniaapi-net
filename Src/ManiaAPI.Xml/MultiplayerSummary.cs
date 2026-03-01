using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public sealed record MultiplayerSummary(
    string Zone, 
    int Count,
    DateTimeOffset Timestamp,
    ImmutableArray<LadderRank> LadderPoints);
