using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public interface IInitServerMP : IAnyServerMP
{
    Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string? login = null, CancellationToken cancellationToken = default);
    Task<WaitingParams> GetWaitingParamsAsync(string? login = null, CancellationToken cancellationToken = default);
}

public abstract class InitServerMP : AnyServer, IInitServerMP
{
    protected InitServerMP(HttpClient client) : base(client) { }

    protected InitServerMP(Uri uri) : base(uri) { }

    public virtual async Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string? login = null, CancellationToken cancellationToken = default)
    {
        return await GetWaitingParamsResponseAsync(Client, GameXml, login, cancellationToken);
    }

    public async Task<WaitingParams> GetWaitingParamsAsync(string? login = null, CancellationToken cancellationToken = default)
    {
        return (await GetWaitingParamsResponseAsync(login, cancellationToken)).Result;
    }

    internal static async Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(HttpClient client, string gameXml, string? login, CancellationToken cancellationToken)
    {
        const string RequestName = "GetWaitingParams";
        var authorXml = login is null ? null : $"<author><login>{login}</login></author>";
        var response = await XmlHelper.SendAsync(client, gameXml, authorXml, RequestName, string.Empty, cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
        {
            var waitingQueueDuration = 0;
            var waitingQueueMessage = default(string);
            var masterServers = ImmutableList.CreateBuilder<MasterServerInfo>();

            while (xml.TryReadStartElement(out var element))
            {
                switch (element)
                {
                    case "wt":
                        waitingQueueDuration = int.Parse(xml.ReadContent());
                        break;
                    case "wm":
                        waitingQueueMessage = xml.ReadContentAsString();
                        break;
                    case "ms":
                        var name = string.Empty;
                        var domain = string.Empty;
                        var path = string.Empty;
                        var portHttps = 443;
                        var portHttp = 80;

                        while (xml.TryReadStartElement(out var valueElement))
                        {
                            switch (valueElement)
                            {
                                case "b":
                                    name = xml.ReadContentAsString();
                                    break;
                                case "c":
                                    domain = xml.ReadContentAsString();
                                    break;
                                case "d":
                                    path = xml.ReadContentAsString();
                                    break;
                                case "e":
                                    portHttps = int.Parse(xml.ReadContent());
                                    break;
                                case "f":
                                    portHttp = int.Parse(xml.ReadContent());
                                    break;
                                default:
                                    xml.ReadContent();
                                    break;
                            }

                            _ = xml.SkipEndElement();
                        }

                        masterServers.Add(new MasterServerInfo(name, domain, path, portHttps, portHttp));
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }

                _ = xml.SkipEndElement();
            }

            return new WaitingParams(waitingQueueDuration, waitingQueueMessage, masterServers.ToImmutable());
        });
    }

    public virtual async Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(ulong steamId, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetAccountFromSteamUserResponseAsync(Client, GameXml, steamId, cancellationToken);
    }

    public virtual async Task<string?> GetAccountFromSteamUserAsync(ulong steamId, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetAccountFromSteamUserAsync(Client, GameXml, steamId, cancellationToken);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetWebIdentityFromManiaplanetLoginResponseAsync(Client, GameXml, logins, cancellationToken);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(params string[] logins)
    {
        return await AnyServerMP.GetWebIdentityFromManiaplanetLoginResponseAsync(Client, GameXml, logins);
    }

    public virtual async Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetWebIdentityFromManiaplanetLoginAsync(Client, GameXml, logins, cancellationToken);
    }

    public virtual async Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(params string[] logins)
    {
        return await AnyServerMP.GetWebIdentityFromManiaplanetLoginAsync(Client, GameXml, logins, cancellationToken: default);
    }

    public virtual async Task<WebIdentityPlayer?> GetWebIdentityFromManiaplanetLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetWebIdentityFromManiaplanetLoginAsync(Client, GameXml, login, cancellationToken);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<WebIdentityLogin>>> GetManiaplanetLoginFromWebIdentityResponseAsync(IEnumerable<WebIdentity> webIdentities, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetManiaplanetLoginFromWebIdentityResponseAsync(Client, GameXml, webIdentities, cancellationToken);
    }

    public virtual async Task<MasterServerResponse<ImmutableList<WebIdentityLogin>>> GetManiaplanetLoginFromWebIdentityResponseAsync(params WebIdentity[] webIdentities)
    {
        return await AnyServerMP.GetManiaplanetLoginFromWebIdentityResponseAsync(Client, GameXml, webIdentities, cancellationToken: default);
    }

    public virtual async Task<ImmutableList<WebIdentityLogin>> GetManiaplanetLoginFromWebIdentityAsync(IEnumerable<WebIdentity> webIdentities, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetManiaplanetLoginFromWebIdentityAsync(Client, GameXml, webIdentities, cancellationToken);
    }

    public virtual async Task<ImmutableList<WebIdentityLogin>> GetManiaplanetLoginFromWebIdentityAsync(params WebIdentity[] webIdentities)
    {
        return await AnyServerMP.GetManiaplanetLoginFromWebIdentityAsync(Client, GameXml, webIdentities, cancellationToken: default);
    }

    public virtual async Task<WebIdentityLogin?> GetManiaplanetLoginFromWebIdentityAsync(WebIdentity webIdentity, CancellationToken cancellationToken = default)
    {
        return await AnyServerMP.GetManiaplanetLoginFromWebIdentityAsync(Client, GameXml, webIdentity, cancellationToken);
    }

    public async Task<WebIdentityLogin?> GetManiaplanetLoginFromWebIdentityAsync(string platform, Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetManiaplanetLoginFromWebIdentityAsync(new WebIdentity(platform, userId), cancellationToken);
    }
}