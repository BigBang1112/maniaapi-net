namespace ManiaAPI.Xml.TMT;

public sealed record AggregatedSummaryProgress<T>(Platform Platform, MasterServerResponse<Summary<T>> Response) where T : struct;
