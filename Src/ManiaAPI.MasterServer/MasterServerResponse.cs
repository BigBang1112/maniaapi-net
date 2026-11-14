namespace ManiaAPI.MasterServer;

public record MasterServerResponse(TimeSpan? ExecutionTime, TimeSpan XmlParseTime, MasterServerResponseDetails Details);

public sealed record MasterServerResponse<T>(T Result, TimeSpan? ExecutionTime, TimeSpan XmlParseTime, MasterServerResponseDetails Details)
    : MasterServerResponse(ExecutionTime, XmlParseTime, Details);