namespace ManiaAPI.GBXRemote;

public sealed record CallbackMessage(string MethodName, object[] MethodParams);