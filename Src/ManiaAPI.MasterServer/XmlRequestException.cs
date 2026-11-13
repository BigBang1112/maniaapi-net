namespace ManiaAPI.MasterServer;

[Serializable]
public class XmlRequestException : Exception
{
    /// <summary>
    /// Raw error code returned by the master server.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Error code returned by the master server. It can hold a value that is not defined in <see cref="MasterServerError"/>, use <see cref="Value"/> for the raw code.
    /// </summary>
    public MasterServerError Error => (MasterServerError)Value;

    /// <summary>
    /// Error message returned by the master server.
    /// </summary>
    public string ErrorMessage { get; }

    public TimeSpan XmlParseTime { get; }
    public XmlResponseDetails Details { get; }

    public XmlRequestException(int value, string message, TimeSpan xmlParseTime, XmlResponseDetails details) : base($"Error {value}: {message}")
    {
        Value = value;
        ErrorMessage = message;
        XmlParseTime = xmlParseTime;
        Details = details;
    }
}
