namespace ManiaAPI.GBXRemote;

public sealed record GBXRemoteCallbackMessage(string MethodName, object[] MethodParams);