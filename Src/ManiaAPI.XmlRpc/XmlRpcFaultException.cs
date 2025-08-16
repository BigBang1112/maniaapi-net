namespace ManiaAPI.XmlRpc;

[Serializable]
internal class XmlRpcFaultException : Exception
{
    public XmlRpcFaultException() { }
    public XmlRpcFaultException(string? message) : base(message) { }
    public XmlRpcFaultException(string? message, Exception inner) : base(message, inner) { }
}