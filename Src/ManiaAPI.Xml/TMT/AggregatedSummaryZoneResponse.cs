using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryZoneResponse<T>(
    ImmutableDictionary<Platform, AggregatedSummaryInfo> Platforms,
    ImmutableArray<AggregatedSummaryZone<T>> Summaries) where T : struct;