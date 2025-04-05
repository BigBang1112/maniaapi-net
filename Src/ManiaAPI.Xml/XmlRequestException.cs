namespace ManiaAPI.Xml;

[Serializable]
public class XmlRequestException : Exception
{
    public int Value { get; }
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
