using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public record Summary<T>(
    DateTimeOffset Timestamp,
    ImmutableList<RecordUnit<T>> Scores) where T : struct;
