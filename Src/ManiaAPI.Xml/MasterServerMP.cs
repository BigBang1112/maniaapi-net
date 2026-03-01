namespace ManiaAPI.Xml;

public interface IMasterServerMP : IMasterServer
{
    Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(ulong steamId, CancellationToken cancellationToken = default);
    Task<string?> GetAccountFromSteamUserAsync(ulong steamId, CancellationToken cancellationToken = default);

    Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string login, CancellationToken cancellationToken = default);
    Task<WaitingParams> GetWaitingParamsAsync(string login, CancellationToken cancellationToken = default);
}

public abstract class MasterServerMP : MasterServer, IMasterServerMP
{
    protected MasterServerMP(Uri uri) : base(uri) { }

    protected MasterServerMP(MasterServerInfo info) : base(info.GetUri()) { }

    protected MasterServerMP(HttpClient client) : base(client) { }

    protected abstract string GetGameXml(string titleId);

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
