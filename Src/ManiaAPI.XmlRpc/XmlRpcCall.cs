namespace ManiaAPI.XmlRpc;

/// <summary>
/// Represents a single method call to be executed as part of a <c>system.multicall</c> request.
/// </summary>
/// <param name="MethodName">The name of the method to call.</param>
/// <param name="Parameters">The parameters to pass to the method.</param>
public readonly record struct XmlRpcCall(string MethodName, params object[] Parameters);
