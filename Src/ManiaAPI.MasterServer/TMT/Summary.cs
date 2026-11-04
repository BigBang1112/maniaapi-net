using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMT;

public record Summary<T>(
    DateTimeOffset Timestamp,
    ImmutableList<RecordUnit<T>> Scores) where T : struct;
