namespace ManiaAPI.XmlRpc;

public sealed record MasterServerInfo(string Name, string Domain, string Path)
{
    public Uri GetUri() => new($"https://{Domain}/{Path}/request.php");
}
