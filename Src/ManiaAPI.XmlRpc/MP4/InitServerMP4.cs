namespace ManiaAPI.XmlRpc.MP4;

public interface IInitServerMP4 : IInitServer;

public class InitServerMP4 : InitServer, IInitServerMP4
{
    public const string DefaultAddress = "http://init.v04.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlRpcHelperMP4.GameXml;

    public InitServerMP4() : base(new Uri(DefaultAddress))
    {
    }

    public InitServerMP4(HttpClient client) : base(client)
    {
    }
}
