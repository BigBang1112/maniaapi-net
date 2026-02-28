namespace ManiaAPI.Xml.MP4;

public interface IInitServerMP4 : IInitServerMP;

public class InitServerMP4 : InitServerMP, IInitServerMP4
{
    public const string DefaultUrl = "https://init.v04.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlHelperMP4.GameXml;

    public InitServerMP4(string url = DefaultUrl) : base(new Uri(url)) { }

    public InitServerMP4(Uri uri) : base(uri) { }

    public InitServerMP4(HttpClient client) : base(client) { }
}
