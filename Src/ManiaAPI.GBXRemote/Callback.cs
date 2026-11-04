namespace ManiaAPI.GBXRemote;

public delegate Task Callback(string methodName, object[] methodParams, CancellationToken cancellationToken = default);