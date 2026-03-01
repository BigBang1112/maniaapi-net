namespace ManiaAPI.Xml.MP3;

public interface IInitServerMP3 : IInitServerMP;

public class InitServerMP3 : InitServerMP, IInitServerMP3
{
    public const string DefaultUrl = "http://init.maniaplanet.com/request.php";

    protected override string GameXml => XmlHelperMP3.GameXml;

    public InitServerMP3(string url = DefaultUrl) : base(new Uri(url)) { }

    public InitServerMP3(Uri uri) : base(uri) { }

    public InitServerMP3(HttpClient client) : base(client) { }
}
