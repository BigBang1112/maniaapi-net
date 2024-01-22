using GBX.NET;
using GBX.NET.Engines.Game;
using ManiaAPI.NadeoAPI.JsonContexts;
using System.Net.Http.Json;
using TmEssentials;

namespace ManiaAPI.NadeoAPI.Extensions.Gbx;

/// <summary>
/// Provides Gbx related extensions to <see cref="INadeoServices"/>.
/// </summary>
public static class NadeoServicesExtensions
{
    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UploadMapAsync(this INadeoServices services, Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        using var bufferedStream = new BufferedStream(stream);

        using var content = CreateContent(bufferedStream, fileName);

        using var response = await services.SendAsync(HttpMethod.Post, "maps/", content, cancellationToken);

        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.MapInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    /// <summary>
    /// Does not work with <see cref="AuthorizationMethod.DedicatedServer"/>.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="mapId"></param>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FormatException">Author login is invalid.</exception>
    /// <exception cref="NadeoAPIResponseException"></exception>
    public static async Task<MapInfo> UpdateMapAsync(this INadeoServices services, Guid mapId, Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fileName);

        using var bufferedStream = new BufferedStream(stream);

        using var content = CreateContent(bufferedStream, fileName);

        using var response = await services.SendAsync(HttpMethod.Post, $"maps/{mapId}", content, cancellationToken);

        return await response.Content.ReadFromJsonAsync(NadeoAPIJsonContext.Default.MapInfo, cancellationToken) ?? throw new Exception("This shouldn't be null.");
    }

    private static MultipartFormDataContent CreateContent(BufferedStream bufferedStream, string fileName)
    {
        var map = GameBox.ParseNodeHeader<CGameCtnChallenge>(bufferedStream);

        bufferedStream.Position = 0;

        return new MultipartFormDataContent
        {
            { new StringContent(map.TMObjective_AuthorTime.GetValueOrDefault().TotalMilliseconds.ToString()), "authorScore" },
            { new StringContent(map.TMObjective_GoldTime.GetValueOrDefault().TotalMilliseconds.ToString()), "goldScore" },
            { new StringContent(map.TMObjective_SilverTime.GetValueOrDefault().TotalMilliseconds.ToString()), "silverScore" },
            { new StringContent(map.TMObjective_BronzeTime.GetValueOrDefault().TotalMilliseconds.ToString()), "bronzeScore" },
            { new StringContent(AccountUtils.ToAccountId(map.AuthorLogin).ToString()), "author" },
            { new StringContent(map.Collection == 26 ? "Stadium" : map.Collection), "collectionName" },
            { new StringContent(map.MapStyle ?? string.Empty), "mapStyle" },
            { new StringContent(map.MapType ?? string.Empty), "mapType" },
            { new StringContent(map.MapUid), "mapUid" },
            { new StringContent(map.MapName), "name" },
            { new StringContent("true"), "isPlayable" },
            { new StreamContent(bufferedStream), "data", fileName }
        };
    }
}
