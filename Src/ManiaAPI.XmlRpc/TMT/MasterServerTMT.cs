namespace ManiaAPI.XmlRpc.TMT;

public interface IMasterServerTMT : IMasterServer;

public class MasterServerTMT : MasterServer, IMasterServerTMT
{
    protected override string GameXml => XmlRpcHelperTMT.GameXml;

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMT"/> using a raw address. Because the address changes quite often and also because there are multiple platforms, it is recommended to use <see cref="InitServerTMT"/> to get the address first.
    /// </summary>
    /// <param name="address">The address given from <see cref="InitServerTMT"/> via <see cref="InitServer.GetWaitingParamsAsync(CancellationToken)"/>, or a custom address.</param>
    public MasterServerTMT(Uri address) : base(address)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMT"/> using a <see cref="MasterServerInfo"/> object. Be careful to use the correct object for the correct platform given from the correct init server.
    /// </summary>
    /// <param name="info">Info about the master server, usually given from <see cref="InitServerTMT"/>.</param>
    public MasterServerTMT(MasterServerInfo info) : base(info.GetUri())
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMT"/> using any <see cref="HttpClient"/>. You need to set the base address yourself.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerTMT(HttpClient client) : base(client)
    {
    }
}
