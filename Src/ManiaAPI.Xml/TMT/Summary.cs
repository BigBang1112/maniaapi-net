using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record Summary<T>(
    string Zone, 
    DateTimeOffset Timestamp,
    ImmutableArray<RecordUnit<T>> Scores) where T : struct;
