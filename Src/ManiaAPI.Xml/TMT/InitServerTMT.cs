namespace ManiaAPI.Xml.TMT;

public interface IInitServerTMT : IInitServer;

public class InitServerTMT : InitServer, IInitServerTMT
{
    protected override string GameXml => XmlHelperTMT.GameXml;

    public InitServerTMT(Platform platform) : base(new Uri(GetDefaultAddress(platform)))
    {
    }

    public InitServerTMT(HttpClient client) : base(client)
    {
    }

    public static string GetDefaultAddress(Platform platform)
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
