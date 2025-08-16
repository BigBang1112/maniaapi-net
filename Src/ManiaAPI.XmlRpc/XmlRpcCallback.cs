namespace ManiaAPI.XmlRpc;

public delegate Task XmlRpcCallback(string methodName, object?[] methodParams);