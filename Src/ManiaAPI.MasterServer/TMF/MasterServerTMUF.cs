namespace ManiaAPI.MasterServer.TMF;

/// <summary>
/// The TrackMania United Forever master server.
/// </summary>
public interface IMasterServerTMUF : IMasterServerTMF;

/// <summary>
/// The TrackMania United Forever master server. Use <see cref="MasterServerTMNF"/> for TrackMania Nations Forever.
/// </summary>
public class MasterServerTMUF : MasterServerTMF, IMasterServerTMUF
{
    /// <summary>
    /// Address of the TrackMania United Forever master server.
    /// </summary>
    public const string DefaultUrl = "http://game.trackmaniaforever.com/online_game/request.php";

    [Obsolete("Use DefaultUrl instead.")]
    public const string DefaultUnitedUrl = DefaultUrl;

    [Obsolete("Use MasterServerTMNF.DefaultUrl instead, with MasterServerTMNF for TrackMania Nations Forever.")]
    public const string DefaultNationsUrl = MasterServerTMNF.DefaultUrl;

    /// <summary>
    /// Creates a client for the master server at <paramref name="url"/>, identified by <paramref name="userAgent"/>.
    /// </summary>
    public MasterServerTMUF(string url, string userAgent) : base(new Uri(url), userAgent) { }

    [Obsolete("Use MasterServerTMUF(MasterServerTMUF.DefaultUrl, userAgent) for TrackMania United Forever, or MasterServerTMNF for TrackMania Nations Forever.")]
    public MasterServerTMUF(MasterServerType type, string userAgent) : this(GetUrl(type), userAgent) { }

    /// <summary>
    /// Creates a client using any <see cref="HttpClient"/> with a base address, identified by <paramref name="userAgent"/>.
    /// </summary>
    public MasterServerTMUF(HttpClient client, string userAgent) : base(client, userAgent) { }

    /// <summary>
    /// Creates a client for the master server at <paramref name="url"/>, the TrackMania United Forever one by default.
    /// </summary>
    public MasterServerTMUF(string url = DefaultUrl) : base(new Uri(url)) { }

    [Obsolete("Use MasterServerTMUF() for TrackMania United Forever, or MasterServerTMNF for TrackMania Nations Forever.")]
    public MasterServerTMUF(MasterServerType type) : this(GetUrl(type)) { }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerTMUF"/> using any <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerTMUF(HttpClient client) : base(client) { }

    [Obsolete("Only for the obsolete MasterServerType constructors.")]
    private static string GetUrl(MasterServerType type) => type switch
    {
        MasterServerType.United => DefaultUrl,
        MasterServerType.Nations => MasterServerTMNF.DefaultUrl,
        _ => throw new ArgumentException("Invalid master server type", nameof(type))
    };
}
