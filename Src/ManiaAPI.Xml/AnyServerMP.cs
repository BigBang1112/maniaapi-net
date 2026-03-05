using System.Collections.Immutable;
using System.Text;

namespace ManiaAPI.Xml;

public interface IAnyServerMP : IAnyServer
{
    Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(ulong steamId, CancellationToken cancellationToken = default);
    Task<string?> GetAccountFromSteamUserAsync(ulong steamId, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(params string[] logins);
    Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default);
    Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(params string[] logins);
    Task<WebIdentityPlayer?> GetWebIdentityFromManiaplanetLoginAsync(string login, CancellationToken cancellationToken = default);
}

internal static class AnyServerMP
{
    public static async Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(HttpClient client, string gameXml, ulong steamId, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetAccountFromSteamUser";
        var response = await XmlHelper.SendAsync(client, gameXml, authorXml: null, RequestName, $"<i>{steamId}</i>", cancellationToken);
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

    public static async Task<string?> GetAccountFromSteamUserAsync(HttpClient client, string gameXml, ulong steamId, CancellationToken cancellationToken = default)
    {
        try
        {
            return (await GetAccountFromSteamUserResponseAsync(client, gameXml, steamId, cancellationToken)).Result;
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

    public static async Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(HttpClient client, string gameXml, IEnumerable<string> logins, CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetWebIdentityFromManiaplanetLogin";

        var sb = new StringBuilder();
        var i = 1;
        foreach (var login in logins)
        {
            sb.Append($"\n<l{i}>{login}</l{i}>");
            i++;
        }

        var response = await XmlHelper.SendAsync(client, gameXml, authorXml: null, RequestName, sb.ToString(), cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
        {
            var players = ImmutableList.CreateBuilder<WebIdentityPlayer>();

            while (xml.TryReadStartElement("p"))
            {
                var login = string.Empty;
                var webIdentities = ImmutableList.CreateBuilder<WebIdentity>();

                while (xml.TryReadStartElement(out var itemElement))
                {
                    switch (itemElement)
                    {
                        case "l":
                            login = xml.ReadContentAsString();
                            break;
                        case "w":
                            var name = string.Empty;
                            var id = string.Empty;

                            while (xml.TryReadStartElement(out var webIdentityElement))
                            {
                                switch (webIdentityElement)
                                {
                                    case "d":
                                        name = xml.ReadContentAsString();
                                        break;
                                    case "i":
                                        id = xml.ReadContentAsString();
                                        break;
                                    default:
                                        xml.ReadContent();
                                        break;
                                }
                                _ = xml.SkipEndElement();
                            }

                            webIdentities.Add(new WebIdentity(name, id));
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                players.Add(new WebIdentityPlayer(login, webIdentities.ToImmutable()));

                _ = xml.SkipEndElement(); // p
            }

            return players.ToImmutable();
        });
    }

    public static async Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(HttpClient client, string gameXml, params string[] logins)
    {
        return await GetWebIdentityFromManiaplanetLoginResponseAsync(client, gameXml, logins, cancellationToken: default);
    }

    public static async Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(HttpClient client, string gameXml, IEnumerable<string> logins, CancellationToken cancellationToken = default)
    {
        return (await GetWebIdentityFromManiaplanetLoginResponseAsync(client, gameXml, logins, cancellationToken)).Result;
    }

    public static async Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(HttpClient client, string gameXml, params string[] logins)
    {
        return await GetWebIdentityFromManiaplanetLoginAsync(client, gameXml, logins, cancellationToken: default);
    }

    public static async Task<WebIdentityPlayer?> GetWebIdentityFromManiaplanetLoginAsync(HttpClient client, string gameXml, string login, CancellationToken cancellationToken = default)
    {
        return (await GetWebIdentityFromManiaplanetLoginAsync(client, gameXml, [login], cancellationToken)).FirstOrDefault();
    }
}
