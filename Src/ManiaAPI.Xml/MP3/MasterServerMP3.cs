namespace ManiaAPI.Xml.MP3;

public interface IMasterServerMP3 : IMasterServer
{
    Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(ulong steamId, CancellationToken cancellationToken = default);
    Task<string?> GetAccountFromSteamUserAsync(ulong steamId, CancellationToken cancellationToken = default);

    Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string login, CancellationToken cancellationToken = default);
    Task<WaitingParams> GetWaitingParamsAsync(string login, CancellationToken cancellationToken = default);
}

public class MasterServerMP3 : MasterServer, IMasterServerMP3
{
    public const string DefaultUrl = "http://mp05.maniaplanet.com/game/request.php";

    protected override string GameXml => XmlHelperMP3.GameXml;

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP3"/> using the expected master server address. In case it's offline, you need to check <see cref="InitServerMP3"/>.
    /// </summary>
    public MasterServerMP3(string url = DefaultUrl) : base(new Uri(url)) { }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP3"/> using a <see cref="MasterServerInfo"/> object. Be careful to use the correct object given from the correct init server.
    /// </summary>
    /// <param name="info">Info about the master server, usually given from <see cref="InitServerMP3"/>.</param>
    public MasterServerMP3(MasterServerInfo info) : base(info.GetUri()) { }

    /// <summary>
    /// Creates a new instance of <see cref="MasterServerMP3"/> using any <see cref="HttpClient"/>. You need to set the base address yourself.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MasterServerMP3(HttpClient client) : base(client) { }

    protected string GetGameXml(string titleId) => $"{GameXml}<title>{titleId}</title>";

    public virtual async Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(ulong steamId, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetAccountFromSteamUser";
        var response = await XmlHelper.SendAsync(Client, GameXml, authorXml: null, RequestName, $"<i>{steamId}</i>", cancellationToken);
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

    public async Task<string?> GetAccountFromSteamUserAsync(ulong steamId, CancellationToken cancellationToken = default)
    {
        try
        {
            return (await GetAccountFromSteamUserResponseAsync(steamId, cancellationToken)).Result;
        }
        catch (XmlRequestException ex)
        {
            if (ex.Value == 404)
            {
                return null;
            }

            throw;
        }
    }

    public virtual async Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string login, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(login);
        return await InitServerMP.GetWaitingParamsResponseAsync(Client, GameXml, login, cancellationToken);
    }

    public async Task<WaitingParams> GetWaitingParamsAsync(string login, CancellationToken cancellationToken = default)
    {
        return (await GetWaitingParamsResponseAsync(login, cancellationToken)).Result;
    }
}
