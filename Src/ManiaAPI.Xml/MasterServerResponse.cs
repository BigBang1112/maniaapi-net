namespace ManiaAPI.Xml;

public record MasterServerResponse(TimeSpan? ExecutionTime, TimeSpan XmlParseTime, XmlResponseDetails Details);

public sealed record MasterServerResponse<T>(T Result, TimeSpan? ExecutionTime, TimeSpan XmlParseTime, XmlResponseDetails Details)
    : MasterServerResponse(ExecutionTime, XmlParseTime, Details);