namespace ManiaAPI.XmlRpc.TMT;

public sealed record AggregatedSummaryInfo(TimeSpan? ExecutionTime, TimeSpan XmlParseTime, XmlRpcResponseDetails Details);
