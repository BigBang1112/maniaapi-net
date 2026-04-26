using System.Collections.Immutable;

namespace ManiaAPI.Xml;

public interface IMasterServer : IAnyServer, IDisposable
{
    Task<MasterServerResponse<ImmutableList<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default);
    Task<ImmutableList<League>> GetLeaguesAsync(CancellationToken cancellationToken = default);
    Task<MasterServerResponse<PlayerInfos>> GetPlayerInfosResponseAsync(string login, CancellationToken cancellationToken = default);
    Task<PlayerInfos> GetPlayerInfosAsync(string login, CancellationToken cancellationToken = default);
    Task<MasterServerResponse<CheckLoginResult>> CheckLoginResponseAsync(string login, CancellationToken cancellationToken = default);
    Task<CheckLoginResult> CheckLoginAsync(string login, CancellationToken cancellationToken = default);
}

public abstract class MasterServer : AnyServer, IMasterServer
{
    protected MasterServer(HttpClient client) : base(client) { }

    protected MasterServer(Uri uri) : base(uri) { }

    public virtual async Task<MasterServerResponse<ImmutableList<League>>> GetLeaguesResponseAsync(CancellationToken cancellationToken = default)
    {
        return await RequestAsync("GetLeagues", authorXml: null, parametersXml: string.Empty, (ref xml) =>
        {
            var items = ImmutableList.CreateBuilder<League>();

            while (xml.TryReadStartElement("l"))
            {
                var name = string.Empty;
                var path = string.Empty;
                var logoUrl = string.Empty;

                while (xml.TryReadStartElement(out var itemElement))
                {
                    switch (itemElement)
                    {
                        case "a":
                            name = xml.ReadContentAsString();
                            break;
                        case "b":
                            path = xml.ReadContentAsString();
                            break;
                        case "i":
                            logoUrl = xml.ReadContentAsString();
                            break;
                        default:
                            xml.ReadContent();
                            break;
                    }

                    _ = xml.SkipEndElement();
                }

                items.Add(new League(name, path, logoUrl));

                _ = xml.SkipEndElement(); // l
            }

            return items.ToImmutable();

        }, cancellationToken);
    }

    public async Task<ImmutableList<League>> GetLeaguesAsync(CancellationToken cancellationToken = default)
    {
        return (await GetLeaguesResponseAsync(cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<PlayerInfos>> GetPlayerInfosResponseAsync(string login, CancellationToken cancellationToken = default)
    {
        return await RequestAsync("GetPlayerInfos", authorXml: null, $"<login>{login}</login>", (ref xml) =>
        {
            var nickname = string.Empty;
            var zone = string.Empty;
            var createdAt = default(DateTimeOffset?);
            var d = 0;
            var lastZoneUpdate = default(DateTimeOffset?);
            var k = default(int?);

            while (xml.TryReadStartElement(out var infoElement))
            {
                switch (infoElement)
                {
                    case "a":
                        login = xml.ReadContentAsString();
                        break;
                    case "b":
                        nickname = xml.ReadContentAsString();
                        break;
                    case "c":
                        zone = xml.ReadContentAsString();
                        break;
                    case "d":
                        d = int.Parse(xml.ReadContent());
                        break;
                    case "e":
                        var e = long.Parse(xml.ReadContent());
                        createdAt = e == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(e);
                        break;
                    case "j":
                        lastZoneUpdate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(xml.ReadContent()));
                        break;
                    case "k":
                        k = int.Parse(xml.ReadContent());
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }
                _ = xml.SkipEndElement();
            }

            return new PlayerInfos(login, nickname, zone, createdAt, d, lastZoneUpdate, k);

        }, cancellationToken);
    }

    public async Task<PlayerInfos> GetPlayerInfosAsync(string login, CancellationToken cancellationToken = default)
    {
        return (await GetPlayerInfosResponseAsync(login, cancellationToken)).Result;
    }

    public virtual async Task<MasterServerResponse<CheckLoginResult>> CheckLoginResponseAsync(string login, CancellationToken cancellationToken = default)
    {
        return await RequestAsync("CheckLogin", authorXml: null, $"<l>{login}</l>", (ref xml) =>
        {
            var exists = false;
            var paid = default(bool?);
            var migrated = false;

            while (xml.TryReadStartElement(out var infoElement))
            {
                switch (infoElement)
                {
                    case "e":
                        exists = MemoryExtensions.Equals(xml.ReadContent(), "1", StringComparison.Ordinal);
                        break;
                    case "p":
                        paid = xml.ReadContentAsBoolean();
                        break;
                    case "m":
                        migrated = MemoryExtensions.Equals(xml.ReadContent(), "1", StringComparison.Ordinal);
                        break;
                    default:
                        xml.ReadContent();
                        break;
                }
                _ = xml.SkipEndElement();
            }

            return new CheckLoginResult(exists, paid, migrated);

        }, cancellationToken);
    }

    public async Task<CheckLoginResult> CheckLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return (await CheckLoginResponseAsync(login, cancellationToken)).Result;
    }
}
