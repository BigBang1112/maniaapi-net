namespace ManiaAPI.XmlRpc;

public sealed record XmlRpcCallbackMessage(string MethodName, object?[] MethodParams);