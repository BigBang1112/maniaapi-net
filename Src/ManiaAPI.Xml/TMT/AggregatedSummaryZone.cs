using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryZone<T>(
    string Zone,
    ImmutableDictionary<Platform, DateTimeOffset> Timestamps,
    ImmutableArray<AggregatedRecordUnit<T>> Scores) : AggregatedSummary<T>(Timestamps, Scores) where T : struct;
