namespace ManiaAPI.Xml;

public sealed record MasterServerResponse<T>(T Result, TimeSpan? ExecutionTime, TimeSpan XmlParseTime, XmlResponseDetails Details);
