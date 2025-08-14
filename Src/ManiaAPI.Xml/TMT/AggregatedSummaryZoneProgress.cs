using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryZoneProgress<T>(Platform Platform, MasterServerResponse<ImmutableArray<SummaryZone<T>>> Response) where T : struct;
