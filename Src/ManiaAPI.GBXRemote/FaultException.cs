namespace ManiaAPI.GBXRemote;

[Serializable]
internal class FaultException : Exception
{
    public FaultException() { }
    public FaultException(string? message) : base(message) { }
    public FaultException(string? message, Exception inner) : base(message, inner) { }
}