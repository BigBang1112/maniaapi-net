namespace ManiaAPI.XmlRpc;

[Serializable]
internal class XmlRpcClientException : Exception
{
    public XmlRpcClientException() { }
    public XmlRpcClientException(string? message) : base(message) { }
    public XmlRpcClientException(string? message, Exception inner) : base(message, inner) { }
}