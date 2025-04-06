using ManiaAPI.Xml;

namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryInfo(TimeSpan? ExecutionTime, TimeSpan XmlParseTime, XmlResponseDetails? Details, string? ErrorMessage);
