using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryZoneResponse<T>(
    ImmutableDictionary<Platform, AggregatedSummaryInfo> Platforms,
    ImmutableList<AggregatedSummaryZone<T>> Summaries) where T : struct;