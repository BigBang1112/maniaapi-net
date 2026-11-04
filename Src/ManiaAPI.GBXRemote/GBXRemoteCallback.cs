namespace ManiaAPI.GBXRemote;

public delegate Task GBXRemoteCallback(string methodName, object[] methodParams, CancellationToken cancellationToken = default);