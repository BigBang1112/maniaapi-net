using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record SummaryZone<T>(
    string Zone, 
    DateTimeOffset Timestamp,
    ImmutableList<RecordUnit<T>> Scores) : Summary<T>(Timestamp, Scores) where T : struct;
