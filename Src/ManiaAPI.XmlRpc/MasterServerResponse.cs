namespace ManiaAPI.XmlRpc;

public sealed record MasterServerResponse<T>(T Result, TimeSpan? ExecutionTime, XmlRpcResponseDetails Details);
