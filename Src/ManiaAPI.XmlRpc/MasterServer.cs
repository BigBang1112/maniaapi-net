namespace ManiaAPI.XmlRpc;

public abstract class MasterServer : IDisposable
{
    public HttpClient Client { get; }

    public MasterServer()
    {
        Client = new();
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
