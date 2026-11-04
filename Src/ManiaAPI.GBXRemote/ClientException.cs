namespace ManiaAPI.GBXRemote;

[Serializable]
internal class ClientException : Exception
{
    public ClientException() { }
    public ClientException(string? message) : base(message) { }
    public ClientException(string? message, Exception inner) : base(message, inner) { }
}