using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public record AggregatedSummary<T>(
    ImmutableDictionary<Platform, DateTimeOffset> Timestamps,
    ImmutableList<AggregatedRecordUnit<T>> Scores) where T : struct;
