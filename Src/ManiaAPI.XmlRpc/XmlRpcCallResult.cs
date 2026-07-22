namespace ManiaAPI.XmlRpc;

/// <summary>
/// Represents the outcome of a single call executed through <c>system.multicall</c>.
/// If the call succeeded, <see cref="Value"/> contains the returned value.
/// If it failed, <see cref="FaultCode"/> and <see cref="FaultString"/> describe the fault.
/// </summary>
/// <param name="Value">The value returned by the method call, if it succeeded.</param>
/// <param name="FaultCode">The fault code, if the call failed.</param>
/// <param name="FaultString">The fault description, if the call failed.</param>
public readonly record struct XmlRpcCallResult(object? Value, int? FaultCode, string? FaultString)
{
    /// <summary>
    /// Indicates whether the call failed.
    /// </summary>
    public bool IsFault => FaultString is not null;
}
