using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMT;

public sealed record AggregatedSummaryZone<T>(
    string Zone,
    ImmutableDictionary<Platform, DateTimeOffset> Timestamps,
    ImmutableList<AggregatedRecordUnit<T>> Scores) : AggregatedSummary<T>(Timestamps, Scores) where T : struct;
