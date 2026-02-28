namespace ManiaAPI.Xml;

public sealed record MasterServerInfo(string Name, string Domain, string Path, int PortHttps, int PortHttp)
{
    public Uri GetUri(bool useHttps = true) => useHttps
        ? new($"https://{Domain}:{PortHttps}/{Path}/request.php")
        : new($"http://{Domain}:{PortHttp}/{Path}/request.php");
}
