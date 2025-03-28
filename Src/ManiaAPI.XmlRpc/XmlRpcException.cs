namespace ManiaAPI.XmlRpc;

[Serializable]
public class XmlRpcException : Exception
{
    public int Value { get; }
    public string ErrorMessage { get; }
    public TimeSpan XmlParseTime { get; }
    public XmlRpcResponseDetails Details { get; }

    public XmlRpcException(int value, string message, TimeSpan xmlParseTime, XmlRpcResponseDetails details) : base($"Error {value}: {message}")
    {
        Value = value;
        ErrorMessage = message;
        XmlParseTime = xmlParseTime;
        Details = details;
    }
}
