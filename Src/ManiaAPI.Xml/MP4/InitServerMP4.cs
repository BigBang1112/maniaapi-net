namespace ManiaAPI.Xml.MP4;

public interface IInitServerMP4 : IInitServerMP;

public class InitServerMP4 : InitServerMP, IInitServerMP4
{
    public const string DefaultAddress = "https://init.v04.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlHelperMP4.GameXml;

    public InitServerMP4() : base(new Uri(DefaultAddress))
    {
    }

    public InitServerMP4(HttpClient client) : base(client)
    {
    }
}
