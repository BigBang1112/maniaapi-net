using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummary<T>(
    string Zone, 
    ImmutableDictionary<Platform, DateTimeOffset> Timestamps,
    ImmutableArray<AggregatedRecordUnit<T>> Scores) where T : struct;
