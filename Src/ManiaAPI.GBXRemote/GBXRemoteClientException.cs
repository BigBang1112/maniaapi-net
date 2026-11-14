namespace ManiaAPI.GBXRemote;

/// <summary>
/// The connection to the game server failed, was closed, or carried a message that couldn't be read.
/// For an error returned by the server itself, see <see cref="GBXRemoteFaultException"/>.
/// </summary>
public class GBXRemoteClientException : Exception
{
    public GBXRemoteClientException() { }
    public GBXRemoteClientException(string? message) : base(message) { }
    public GBXRemoteClientException(string? message, Exception? innerException) : base(message, innerException) { }
}
