namespace ManiaAPI.MasterServer.TMF;

/// <summary>
/// The TrackMania Nations Forever master server.
/// </summary>
public interface IMasterServerTMNF : IMasterServerTMF;

/// <summary>
/// The TrackMania Nations Forever master server. Use <see cref="MasterServerTMUF"/> for TrackMania United Forever.
/// </summary>
public class MasterServerTMNF : MasterServerTMF, IMasterServerTMNF
{
    /// <summary>
    /// Address of the TrackMania Nations Forever master server.
    /// </summary>
    public const string DefaultUrl = "http://game2.trackmaniaforever.com/online_game/request.php";

    /// <summary>
    /// Creates a client for the master server at <paramref name="url"/>, identified by <paramref name="userAgent"/>.
    /// </summary>
    public MasterServerTMNF(string url, string userAgent) : base(new Uri(url), userAgent) { }

    /// <summary>
    /// Creates a client using any <see cref="HttpClient"/> with a base address, identified by <paramref name="userAgent"/>.
    /// </summary>
    public MasterServerTMNF(HttpClient client, string userAgent) : base(client, userAgent) { }

    /// <summary>
    /// Creates a client for the master server at <paramref name="url"/>, the TrackMania Nations Forever one by default.
    /// </summary>
    public MasterServerTMNF(string url = DefaultUrl) : base(new Uri(url)) { }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMNF"/> using any <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerTMNF(HttpClient client) : base(client) { }
}
