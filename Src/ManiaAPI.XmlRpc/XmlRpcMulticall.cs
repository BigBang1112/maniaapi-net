namespace ManiaAPI.XmlRpc;

/// <summary>
/// Represents a single method call to be executed as part of a <c>system.multicall</c> request.
/// </summary>
/// <param name="MethodName">The name of the method to call.</param>
/// <param name="Parameters">The parameters to pass to the method.</param>
public readonly record struct XmlRpcMulticall(string MethodName, object?[] Parameters)
{
    /// <summary>
    /// Creates a multicall entry for a method that takes no parameters.
    /// </summary>
    /// <param name="methodName">The name of the method to call.</param>
    public XmlRpcMulticall(string methodName) : this(methodName, []) { }
}
