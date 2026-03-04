using MinimalXmlReader;
using System.Collections.Immutable;
using System.Text;

namespace ManiaAPI.Xml;

public interface IMasterServerMP : IMasterServer
{
    Task<MasterServerResponse<WaitingParams>> GetWaitingParamsResponseAsync(string login, CancellationToken cancellationToken = default);
    Task<WaitingParams> GetWaitingParamsAsync(string login, CancellationToken cancellationToken = default);

    Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(ulong steamId, CancellationToken cancellationToken = default);
    Task<string?> GetAccountFromSteamUserAsync(ulong steamId, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(params string[] logins);
    Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default);
    Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(params string[] logins);
    Task<WebIdentityPlayer?> GetWebIdentityFromManiaplanetLoginAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple multiplayer leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="zones">The list of zones.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the multiplayer summaries.</returns>
    Task<MasterServerResponse<ImmutableList<MultiplayerSummary>>> GetMultiplayerLeaderBoardSummariesResponseAsync(string titleId, IEnumerable<string> zones, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple multiplayer leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="zones">The list of zones.</param>
    /// <returns>A task with the result containing the multiplayer summaries.</returns>
    Task<MasterServerResponse<ImmutableList<MultiplayerSummary>>> GetMultiplayerLeaderBoardSummariesResponseAsync(string titleId, params string[] zones);

    /// <summary>
    /// Gets multiple multiplayer leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="zones">The list of zones.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task with the result containing the multiplayer summaries.</returns>
    Task<ImmutableList<MultiplayerSummary>> GetMultiplayerLeaderBoardSummariesAsync(string titleId, IEnumerable<string> zones, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple multiplayer leaderboard summaries in a single request.
    /// </summary>
    /// <param name="titleId">The title ID of a title pack.</param>
    /// <param name="zones">The list of zones.</param>
    /// <returns>A task with the result containing the multiplayer summaries.</returns>
    Task<ImmutableList<MultiplayerSummary>> GetMultiplayerLeaderBoardSummariesAsync(string titleId, params string[] zones);
}

public abstract class MasterServerMP : MasterServer, IMasterServerMP
{
    protected MasterServerMP(Uri uri) : base(uri) { }

    protected MasterServerMP(MasterServerInfo info) : base(info.GetUri()) { }

    protected MasterServerMP(HttpClient client) : base(client) { }

    protected abstract string GetGameXml(string titleId);

    public virtual async Task<MasterServerResponse<string>> GetAccountFromSteamUserResponseAsync(ulong steamId, CancellationToken cancellationToken = default)
    {
        return await InitServerMP.GetAccountFromSteamUserResponseAsync(Client, GameXml, steamId, cancellationToken);
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

    public virtual async Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default)
    {
        return await InitServerMP.GetWebIdentityFromManiaplanetLoginResponseAsync(Client, GameXml, logins, cancellationToken);
    }

    public async Task<MasterServerResponse<ImmutableList<WebIdentityPlayer>>> GetWebIdentityFromManiaplanetLoginResponseAsync(params string[] logins)
    {
        return await GetWebIdentityFromManiaplanetLoginResponseAsync(logins, cancellationToken: default);
    }

    public async Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default)
    {
        return (await GetWebIdentityFromManiaplanetLoginResponseAsync(logins, cancellationToken)).Result;
    }

    public async Task<ImmutableList<WebIdentityPlayer>> GetWebIdentityFromManiaplanetLoginAsync(params string[] logins)
    {
        return await GetWebIdentityFromManiaplanetLoginAsync(logins, cancellationToken: default);
    }

    public async Task<WebIdentityPlayer?> GetWebIdentityFromManiaplanetLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return (await GetWebIdentityFromManiaplanetLoginAsync([login], cancellationToken)).FirstOrDefault();
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

    public virtual async Task<MasterServerResponse<ImmutableList<MultiplayerSummary>>> GetMultiplayerLeaderBoardSummariesResponseAsync(
        string titleId,
        IEnumerable<string> zones,
        CancellationToken cancellationToken = default)
    {
        const string RequestName = "GetMultiplayerLeaderBoardSummaries";

        var sb = new StringBuilder();
        var i = 1;
        foreach (var zone in zones)
        {
            sb.Append($"\n<z{i}>{zone}</z{i}>");
            i++;
        }

        var response = await XmlHelper.SendAsync(Client, GetGameXml(titleId), authorXml: null, RequestName, sb.ToString(), cancellationToken);
        return XmlHelper.ProcessResponseResult(RequestName, response, (ref xml) =>
        {
            var summaries = ImmutableList.CreateBuilder<MultiplayerSummary>();

            while (xml.TryReadStartElement("s"))
            {
                var zone = string.Empty;
                var timestamp = DateTimeOffset.UtcNow;
                var count = 0;
                var ladderPointsBuilder = ImmutableArray.CreateBuilder<LadderRank>();

                while (xml.TryReadStartElement(out var itemElement))
                {
                    switch (itemElement)
                    {
                        case "z":
                            zone = xml.ReadContentAsString();
                            break;
                        case "d":
                            timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                            break;
                        case "c":
                            count = int.Parse(xml.ReadContent());
                            break;
                        case "i":
                            ladderPointsBuilder.Add(ReadLadderRank(ref xml));
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                summaries.Add(new MultiplayerSummary(zone, count, timestamp, ladderPointsBuilder.ToImmutable()));

                _ = xml.SkipEndElement(); // s
            }

            return summaries.ToImmutable();
        });
    }

    public async Task<MasterServerResponse<ImmutableList<MultiplayerSummary>>> GetMultiplayerLeaderBoardSummariesResponseAsync(
        string titleId,
        params string[] zones)
    {
        return await GetMultiplayerLeaderBoardSummariesResponseAsync(titleId, zones, cancellationToken: default);
    }

    public async Task<ImmutableList<MultiplayerSummary>> GetMultiplayerLeaderBoardSummariesAsync(
        string titleId,
        IEnumerable<string> zones,
        CancellationToken cancellationToken = default)
    {
        return (await GetMultiplayerLeaderBoardSummariesResponseAsync(titleId, zones, cancellationToken)).Result;
    }

    public async Task<ImmutableList<MultiplayerSummary>> GetMultiplayerLeaderBoardSummariesAsync(
        string titleId,
        params string[] zones)
    {
        return await GetMultiplayerLeaderBoardSummariesAsync(titleId, zones, cancellationToken: default);
    }

    public static LadderRank ReadLadderRank(ref MiniXmlReader xml)
    {
        var score = 0.0;
        var rank = 0;

        while (xml.TryReadStartElement(out var element))
        {
            switch (element)
            {
                case "s":
                    score = double.Parse(xml.ReadContent(), System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "r":
                    rank = int.Parse(xml.ReadContent());
                    break;
                default:
                    xml.ReadContent();
                    break;
            }

            _ = xml.SkipEndElement();
        }

        return new LadderRank(score, rank);
    }
}
