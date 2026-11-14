namespace ManiaAPI.GBXRemote;

/// <summary>
/// The game server answered a call with an XML-RPC fault, like wrong credentials or an unknown login.
/// <see cref="Exception.Message"/> holds the fault string.
/// </summary>
public class GBXRemoteFaultException : Exception
{
    /// <summary>
    /// Fault code returned by the server, like <c>-1000</c>, or <see langword="null"/> if the fault had none.
    /// </summary>
    public int? FaultCode { get; }

    /// <summary>
    /// Fault string returned by the server, like <c>Login unknown.</c>
    /// </summary>
    public string? FaultString { get; }

    public GBXRemoteFaultException(int? faultCode, string? faultString)
        : base(faultString ?? (faultCode.HasValue ? $"Fault {faultCode}" : "Unknown fault"))
    {
        FaultCode = faultCode;
        FaultString = faultString;
    }

    public GBXRemoteFaultException() { }
    public GBXRemoteFaultException(string? message) : base(message) { }
    public GBXRemoteFaultException(string? message, Exception? innerException) : base(message, innerException) { }
}
