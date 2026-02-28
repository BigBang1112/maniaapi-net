namespace ManiaAPI.Xml.MP3;

public interface IMasterServerMP3 : IMasterServerMP;

public class MasterServerMP3 : MasterServerMP, IMasterServerMP3
{
    public const string DefaultUrl = "http://mp05.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlHelperMP3.GameXml;
    protected override string GetGameXml(string titleId) => $"{GameXml}<title>{titleId}</title>";

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP3"/> using the expected master server address. In case it's offline, you need to check <see cref="InitServerMP3"/>.
    /// </summary>
    public MasterServerMP3(string url = DefaultUrl) : base(new Uri(url)) { }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP3"/> using a <see cref="MasterServerInfo"/> object. Be careful to use the correct object given from the correct init server.
    /// </summary>
    /// <param name="info">Info about the master server, usually given from <see cref="InitServerMP3"/>.</param>
    public MasterServerMP3(MasterServerInfo info) : base(info.GetUri()) { }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP3"/> using any <see cref="HttpClient"/>. You need to set the base address yourself.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerMP3(HttpClient client) : base(client) { }
}
