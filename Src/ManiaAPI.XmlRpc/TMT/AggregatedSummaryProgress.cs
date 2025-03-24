using System.Collections.Immutable;

namespace ManiaAPI.XmlRpc.TMT;

public sealed record AggregatedSummaryProgress<T>(Platform Platform, MasterServerResponse<ImmutableArray<Summary<T>>> Response) where T : struct;
