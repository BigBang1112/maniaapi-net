namespace ManiaAPI.XmlRpc;

public abstract class MasterServer : IDisposable
{
    public HttpClient Client { get; }

    public MasterServer()
    {
        Client = new();
    }

    public MasterServer(HttpClient httpClient)
    {
        Client = httpClient;
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
