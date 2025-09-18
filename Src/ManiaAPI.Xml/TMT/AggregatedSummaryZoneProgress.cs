using System.Collections.Immutable;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryZoneProgress<T>(Platform Platform, MasterServerResponse<ImmutableList<SummaryZone<T>>> Response) where T : struct;
