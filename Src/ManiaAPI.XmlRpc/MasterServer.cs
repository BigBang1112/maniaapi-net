namespace ManiaAPI.XmlRpc;

public abstract class MasterServer : IDisposable
{
    public HttpClient Client { get; }

    protected MasterServer(HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "ManiaAPI.NET (XmlRpc) by BigBang1112");
    }

    protected MasterServer() : this(new HttpClient()) { }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
