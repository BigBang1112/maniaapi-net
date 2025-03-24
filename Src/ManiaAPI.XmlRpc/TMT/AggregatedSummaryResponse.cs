﻿using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc.TMT;

public sealed record AggregatedSummaryResponse<T>(
    ImmutableDictionary<Platform, AggregatedSummaryInfo> Platforms,
    ImmutableArray<AggregatedSummary<T>> Summaries) where T : struct;