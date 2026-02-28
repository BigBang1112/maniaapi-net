namespace ManiaAPI.Xml.TMT;

public interface IInitServerTMT : IInitServerMP;

public class InitServerTMT : InitServerMP, IInitServerTMT
{
    protected override string GameXml => XmlHelperTMT.GameXml;

    public InitServerTMT(Platform platform) : base(new Uri(GetDefaultUrl(platform))) { }

    public InitServerTMT(Uri uri) : base(uri) { }

    public InitServerTMT(HttpClient client) : base(client) { }

    public static string GetDefaultUrl(Platform platform)
    {
        var platformStr = platform switch
        {
            Platform.PC => "pc",
            Platform.XB1 => "xb1",
            Platform.PS4 => "ps4",
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

        return $"https://init-{platformStr}.turbo.trackmania.com/game/request.php";
    }
}
