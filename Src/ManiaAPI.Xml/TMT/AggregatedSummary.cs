using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public record AggregatedSummary<T>(
    ImmutableDictionary<Platform, DateTimeOffset> Timestamps,
    ImmutableArray<AggregatedRecordUnit<T>> Scores) where T : struct;
