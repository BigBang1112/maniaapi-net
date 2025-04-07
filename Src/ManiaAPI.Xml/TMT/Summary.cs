using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public record Summary<T>(
    DateTimeOffset Timestamp,
    ImmutableArray<RecordUnit<T>> Scores) where T : struct;
