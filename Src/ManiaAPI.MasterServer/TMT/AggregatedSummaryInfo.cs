using ManiaAPI.MasterServer;

namespace ManiaAPI.MasterServer.TMT;

public sealed record AggregatedSummaryInfo(TimeSpan? ExecutionTime, TimeSpan XmlParseTime, XmlResponseDetails? Details, string? ErrorMessage);
