using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMT;

public sealed record SummaryZone<T>(
    string Zone, 
    DateTimeOffset Timestamp,
    ImmutableList<RecordUnit<T>> Scores) : Summary<T>(Timestamp, Scores) where T : struct;
