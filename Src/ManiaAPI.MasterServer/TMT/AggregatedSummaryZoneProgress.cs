using System.Collections.Immutable;

namespace ManiaAPI.MasterServer.TMT;

public sealed record AggregatedSummaryZoneProgress<T>(Platform Platform, MasterServerResponse<ImmutableList<SummaryZone<T>>> Response) where T : struct;
