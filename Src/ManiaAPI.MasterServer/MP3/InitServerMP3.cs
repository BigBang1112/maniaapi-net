namespace ManiaAPI.MasterServer.MP3;

public interface IInitServerMP3 : IInitServerMP;

public class InitServerMP3 : InitServerMP, IInitServerMP3
{
    public const string DefaultUrl = "http://init.maniaplanet.com/request.php";

    protected override string GameXml => XmlHelperMP3.GameXml;

    public InitServerMP3(string url, string userAgent) : base(new Uri(url), userAgent) { }

    public InitServerMP3(Uri uri, string userAgent) : base(uri, userAgent) { }

    public InitServerMP3(HttpClient client, string userAgent) : base(client, userAgent) { }

    public InitServerMP3(string url = DefaultUrl) : base(new Uri(url)) { }

    public InitServerMP3(Uri uri) : base(uri) { }

    public InitServerMP3(HttpClient client) : base(client) { }
}
