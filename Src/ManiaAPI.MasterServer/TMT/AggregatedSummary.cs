using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMT;

public record AggregatedSummary<T>(
    ImmutableDictionary<Platform, DateTimeOffset> Timestamps,
    ImmutableList<AggregatedRecordUnit<T>> Scores) where T : struct;
