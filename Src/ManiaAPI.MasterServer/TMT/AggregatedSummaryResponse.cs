using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMT;

public sealed record AggregatedSummaryResponse<T>(
    ImmutableDictionary<Platform, AggregatedSummaryInfo> Platforms,
    AggregatedSummary<T> Summary) where T : struct;