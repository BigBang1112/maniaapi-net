﻿using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryResponse<T>(
    ImmutableDictionary<Platform, AggregatedSummaryInfo> Platforms,
    AggregatedSummary<T> Summary) where T : struct;