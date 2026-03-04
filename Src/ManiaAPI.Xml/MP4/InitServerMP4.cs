namespace ManiaAPI.Xml.MP4;

public interface IInitServerMP4 : IInitServerMP
{
    Task<MasterServerResponse<string>> GetAccountFromUplayUserResponseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string?> GetAccountFromUplayUserAsync(Guid id, CancellationToken cancellationToken = default);
}

public class InitServerMP4 : InitServerMP, IInitServerMP4
{
    public const string DefaultUrl = "https://init.v04.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlHelperMP4.GameXml;

    public InitServerMP4(string url = DefaultUrl) : base(new Uri(url)) { }

    public InitServerMP4(Uri uri) : base(uri) { }

    public InitServerMP4(HttpClient client) : base(client) { }

    public virtual async Task<MasterServerResponse<string>> GetAccountFromUplayUserResponseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetAccountFromUplayUserResponseAsync(Client, GameXml, id, cancellationToken);
    }

    public async Task<string?> GetAccountFromUplayUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return (await GetAccountFromUplayUserResponseAsync(id, cancellationToken)).Result;
    }

    internal static async Task<MasterServerResponse<string>> GetAccountFromUplayUserResponseAsync(HttpClient client, string gameXml, Guid id, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetAccountFromUplayUser";
        var response = await XmlHelper.SendAsync(client, gameXml, authorXml: null, RequestName, $"<i>{id}</i>", cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
        {
            while (xml.TryReadStartElement(out var infoElement))
            {
                switch (infoElement)
                {
                    case "l":
                        return xml.ReadContentAsString();
                }
                _ = xml.SkipEndElement();
            }

            throw new InvalidOperationException("Unexpected response format: missing 'l' element.");
        });
    }
}
