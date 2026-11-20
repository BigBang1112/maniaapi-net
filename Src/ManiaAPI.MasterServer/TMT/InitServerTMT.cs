namespace ManiaAPI.MasterServer.TMT;

public interface IInitServerTMT : IInitServerMP;

public class InitServerTMT : InitServerMP, IInitServerTMT
{
    protected override string GameXml => XmlHelperTMT.GameXml;

    public InitServerTMT(Platform platform, string userAgent) : base(new Uri(GetDefaultUrl(platform)), userAgent) { }

    public InitServerTMT(Uri uri, string userAgent) : base(uri, userAgent) { }

    public InitServerTMT(HttpClient client, string userAgent) : base(client, userAgent) { }

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
