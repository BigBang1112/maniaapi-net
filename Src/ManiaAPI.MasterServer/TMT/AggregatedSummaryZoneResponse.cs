using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMT;

public sealed record AggregatedSummaryZoneResponse<T>(
    ImmutableDictionary<Platform, AggregatedSummaryInfo> Platforms,
    ImmutableList<AggregatedSummaryZone<T>> Summaries) where T : struct;